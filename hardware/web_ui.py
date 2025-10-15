#!/usr/bin/env python3
import asyncio
import json
import pathlib
from aiohttp import web

import api
import os
import ssl

ROOT = pathlib.Path(__file__).parent

async def websocket_handler(request):
    ws = web.WebSocketResponse()
    await ws.prepare(request)
    loop = asyncio.get_running_loop()
    # register websocket and store its room (from query string)
    app = request.app
    if 'sockets' not in app:
        # map websocket -> room (None means admin/all)
        app['sockets'] = {}
    if 'cancel_events' not in app:
        # map websocket -> threading.Event used to cancel blocking detect loops
        app['cancel_events'] = {}
    import threading
    # detect room parameter from the ws request URL (e.g. /ws?room=pc0)
    conn_room = None
    try:
        conn_room = request.rel_url.query.get('room')
        if conn_room:
            conn_room = conn_room.lower()
    except Exception:
        conn_room = None
    app['sockets'][ws] = conn_room
    # create a cancel event for this websocket
    cancel_ev = threading.Event()
    app['cancel_events'][ws] = cancel_ev

    async for msg in ws:
        if msg.type == web.WSMsgType.TEXT:
            text = msg.data.strip()
            cmd = text.lower().split()
            # commands: 'enter', 'enter pc0', 'leave', 'leave pc1', 'get_last'
            if cmd[0] == 'enter':
                # use explicit room argument or fallback to the websocket's registered room
                room = None
                if len(cmd) > 1:
                    room = cmd[1]
                if room is None:
                    room = conn_room
                # detect card first, then ask client to provide picture for verification
                await ws.send_str('Waiting for card in enter mode...')
                cancel_ev = app['cancel_events'].get(ws)
                def do_detect():
                    pn_i2c, pn_uart = api.init_readers_for(room)
                    return api.detect_card_for_room(pn_i2c, pn_uart, room=room, timeout=30.0, cancel_event=cancel_ev)
                sensor, uid, data = await loop.run_in_executor(None, do_detect)
                if sensor is None:
                    # timeout
                    await ws.send_str(json.dumps({'event': 'enter_timeout'}))
                elif sensor == 'CANCELLED':
                    await ws.send_str(json.dumps({'event': 'enter_cancelled'}))
                else:
                    # Fetch reference image from API
                    import requests
                    reference_image = None
                    try:
                        # Get room_id for API call
                        sensor_name = 'PC1' if sensor == 'I2C' else 'PC2' if sensor == 'UART' else sensor
                        room_id = 'PC0' if sensor_name == 'PC1' or room == 'pc0' else 'PC1'
                        
                        # Call /api/Keys/image with hash and roomId
                        image_url = 'http://192.168.153.78:5189/api/Keys/image'
                        payload = {'hash': data, 'roomId': room_id}
                        resp = requests.post(image_url, json=payload, timeout=10)
                        if resp.status_code == 200:
                            # Response is JSON: {"message": "Authorized", "image": "base64..."}
                            resp_data = resp.json()
                            reference_image = resp_data.get('image')
                            print(f'Reference image fetched: {reference_image[:50] if reference_image else None}...')
                    except Exception as e:
                        print(f'Failed to fetch reference image: {e}')
                        reference_image = None
                    
                    # store pending card for this websocket until client confirms with photo
                    # Store the full reference_image on server, only send a flag to frontend
                    pending = {'sensor': sensor, 'uid': uid, 'data': data, 'room': room, 'referenceImage': reference_image}
                    print(f'[DEBUG] Storing referenceImage on server: {reference_image is not None}')
                    if reference_image:
                        print(f'[DEBUG] referenceImage length: {len(reference_image)}')
                    app.setdefault('pending_cards', {})[ws] = pending
                    
                    # Send to frontend WITHOUT the huge base64 string - just send a flag
                    card_info = {'sensor': sensor, 'uid': uid, 'data': data, 'room': room, 'hasReferenceImage': reference_image is not None}
                    await ws.send_str(json.dumps({'event': 'card_read', 'card': card_info}))

            elif cmd[0] == 'leave':
                room = None
                if len(cmd) > 1:
                    room = cmd[1]
                if room is None:
                    room = conn_room
                # detect card first for leave, then ask client to provide picture for verification (or confirm)
                await ws.send_str('Waiting for card in leave mode...')
                cancel_ev = app['cancel_events'].get(ws)
                def do_detect_leave():
                    pn_i2c, pn_uart = api.init_readers_for(room)
                    return api.detect_card_for_room(pn_i2c, pn_uart, room=room, timeout=30.0, cancel_event=cancel_ev)
                sensor, uid, data = await loop.run_in_executor(None, do_detect_leave)
                if sensor is None:
                    await ws.send_str(json.dumps({'event': 'leave_timeout'}))
                elif sensor == 'CANCELLED':
                    await ws.send_str(json.dumps({'event': 'leave_cancelled'}))
                else:
                    # Fetch reference image from API
                    import requests
                    reference_image = None
                    try:
                        # Get room_id for API call
                        sensor_name = 'PC1' if sensor == 'I2C' else 'PC2' if sensor == 'UART' else sensor
                        room_id = 'PC0' if sensor_name == 'PC1' or room == 'pc0' else 'PC1'
                        
                        # Call /api/Keys/image with hash and roomId
                        image_url = 'http://192.168.153.78:5189/api/Keys/image'
                        payload = {'hash': data, 'roomId': room_id}
                        resp = requests.post(image_url, json=payload, timeout=10)
                        if resp.status_code == 200:
                            # Response is JSON: {"message": "Authorized", "image": "base64..."}
                            resp_data = resp.json()
                            reference_image = resp_data.get('image')
                            print(f'Reference image fetched (leave): {reference_image[:50] if reference_image else None}...')
                    except Exception as e:
                        print(f'Failed to fetch reference image: {e}')
                        reference_image = None
                    
                    # Store the full reference_image on server, only send a flag to frontend
                    pending = {'sensor': sensor, 'uid': uid, 'data': data, 'room': room, 'referenceImage': reference_image}
                    print(f'[DEBUG] Storing referenceImage on server (leave): {reference_image is not None}')
                    if reference_image:
                        print(f'[DEBUG] referenceImage length: {len(reference_image)}')
                    app.setdefault('pending_cards', {})[ws] = pending
                    
                    # Send to frontend WITHOUT the huge base64 string
                    card_info = {'sensor': sensor, 'uid': uid, 'data': data, 'room': room, 'hasReferenceImage': reference_image is not None}
                    await ws.send_str(json.dumps({'event': 'card_read', 'card': card_info}))

            elif cmd[0] == 'get_last':
                # return the last entry only for this websocket's room
                key = conn_room or 'unknown'
                last = api.last_entries.get(key)
                await ws.send_str(json.dumps({'event': 'last_entry', 'last_entry': last}))
            elif cmd[0] == 'enter_confirm':
                # client confirms the photo and asks server to send the final enter POST
                pending = app.get('pending_cards', {}).pop(ws, None)
                if not pending:
                    await ws.send_str(json.dumps({'event': 'error', 'message': 'no_pending_card'}))
                else:
                    # prepare payload with hash and roomId
                    sensor_name = 'PC1' if pending.get('sensor') == 'I2C' else 'PC2' if pending.get('sensor') == 'UART' else pending.get('sensor')
                    room_name = pending.get('room') or sensor_name.lower()
                    # Format: PC0 or PC1
                    room_id = 'PC0' if sensor_name == 'PC1' or room_name == 'pc0' else 'PC1'
                    enter_payload = {
                        'hash': pending.get('data'),
                        'roomId': room_id
                    }
                    # Keep last_entry for backwards compatibility
                    last_entry = {'Room': sensor_name, 'hash': pending.get('data'), 'uid': pending.get('uid'), 'room': room_name, 'roomId': room_id}
                    api.last_entries[room_name] = last_entry
                    resp = api.send_post(api.ENTER_URL, enter_payload)
                    try:
                        content = resp.json() if resp is not None else None
                    except Exception:
                        content = resp.text if resp is not None else None
                    payload = {'event': 'enter_done' if resp is not None and 200 <= resp.status_code < 300 else 'enter_error', 'result': {'entry': enter_payload, 'code': resp.status_code if resp is not None else None, 'response': content}, 'last_entry': last_entry}
                    await ws.send_str(json.dumps(payload))
            elif cmd[0] == 'leave_confirm':
                pending = app.get('pending_cards', {}).pop(ws, None)
                if not pending:
                    await ws.send_str(json.dumps({'event': 'error', 'message': 'no_pending_card'}))
                else:
                    sensor_name = 'PC1' if pending.get('sensor') == 'I2C' else 'PC2' if pending.get('sensor') == 'UART' else pending.get('sensor')
                    room_name = pending.get('room') or sensor_name.lower()
                    # Format: PC0 or PC1
                    room_id = 'PC0' if sensor_name == 'PC1' or room_name == 'pc0' else 'PC1'
                    leave_payload = {
                        'hash': pending.get('data'),
                        'roomId': room_id
                    }
                    # Keep leave_card for backwards compatibility
                    leave_card = {'Room': sensor_name, 'Key': pending.get('data'), 'uid': pending.get('uid'), 'room': room_name, 'roomId': room_id}
                    api.last_entries[room_name] = leave_card
                    resp = api.send_post(api.LEAVE_URL, leave_payload)
                    try:
                        content = resp.json() if resp is not None else None
                    except Exception:
                        content = resp.text if resp is not None else None
                    payload = {'event': 'leave_done' if resp is not None and 200 <= resp.status_code < 300 else 'leave_error', 'result': {'leave': leave_payload, 'code': resp.status_code if resp is not None else None, 'response': content}, 'last_entry': leave_card}
                    await ws.send_str(json.dumps(payload))
            elif cmd[0] == 'cancel':
                # set the cancel event for this websocket so any running detect loop can stop
                ev = app['cancel_events'].get(ws)
                if ev is not None:
                    ev.set()
                try:
                    await ws.send_str(json.dumps({'event': 'cancelled'}))
                except Exception:
                    pass
            else:
                await ws.send_str('Unknown command')

        elif msg.type == web.WSMsgType.ERROR:
            print('ws connection closed with exception %s' % ws.exception())

    # cleanup
    app['sockets'].pop(ws, None)
    ev = app.get('cancel_events', {}).pop(ws, None)
    if ev is not None:
        try:
            ev.set()
        except Exception:
            pass
    return ws


async def index(request):
    return web.FileResponse(ROOT / 'static' / 'index.html')


async def capture_handler(request):
    """Accept JSON { room: 'pc0', image: 'data:image/jpeg;base64,...' } and save the image to static/images/captures

    Returns JSON { status: 'ok', path: '<filename>' }
    """
    try:
        data = await request.json()
    except Exception:
        return web.json_response({'status': 'error', 'message': 'invalid_json'}, status=400)
    image = data.get('image')
    room = (data.get('room') or 'unknown')
    
    # Get reference image from pending_cards
    # Find the pending card for this room
    app = request.app
    pending_cards = app.get('pending_cards', {})
    reference_image = None
    for ws, pending in pending_cards.items():
        if pending.get('room') == room:
            reference_image = pending.get('referenceImage')
            print(f'[DEBUG] Found pending card for room {room}, has referenceImage: {reference_image is not None}')
            break
    if not image:
        return web.json_response({'status': 'error', 'message': 'no_image'}, status=400)
    import base64, time, os
    # data URL? split header
    if ',' in image:
        header, b64 = image.split(',', 1)
    else:
        header = None
        b64 = image
    ext = 'jpg'
    if header and 'png' in header:
        ext = 'png'
    ts = int(time.time())
    captures_dir = ROOT / 'static' / 'images' / 'captures'
    captures_dir.mkdir(parents=True, exist_ok=True)
    fname = f"{room}_{ts}.{ext}"
    fpath = captures_dir / fname
    try:
        with open(fpath, 'wb') as f:
            f.write(base64.b64decode(b64))
    except Exception as e:
        return web.json_response({'status': 'error', 'message': str(e)}, status=500)

    # If analyze.compare_faces is available, compare the captured image to the reference image from API
    compare_result = None
    print(f'[DEBUG] capture_handler: reference_image received: {reference_image is not None}')
    if reference_image:
        print(f'[DEBUG] Reference image length: {len(reference_image)}')
        print(f'[DEBUG] Reference image first 100 chars: {reference_image[:100]}')
    else:
        print(f'[DEBUG] reference_image is None or empty!')
    
    if reference_image:
        try:
            from analyze import compare_faces
            from reconstruct_image import reconstruct_image_from_base64_string
            
            # Step 1: Save the base64 string to a temporary file
            b64_temp_file = captures_dir / f"{room}_b64_{ts}.txt"
            with open(b64_temp_file, 'w') as f:
                f.write(reference_image)
            print(f'Base64 string saved to: {b64_temp_file}')
            
            # Step 2: Reconstruct image from the base64 file
            ref_fname = f"{room}_ref_{ts}.{ext}"
            ref_fpath = captures_dir / ref_fname
            
            success, error = reconstruct_image_from_base64_string(reference_image, str(ref_fpath))
            
            if not success:
                raise Exception(f'Failed to reconstruct image: {error}')
            
            print(f'Reference image reconstructed: {ref_fpath}, file size: {os.path.getsize(ref_fpath)} bytes')
            
            # Step 3: Compare the two images
            captured_path = str(fpath)
            ref_path = str(ref_fpath)
            print(f'Comparing images: {captured_path} vs {ref_path}')
            compare_result = compare_faces(captured_path, ref_path)
            print(f'Compare result: {compare_result}')
            
            # Clean up temporary files
            try:
                os.remove(b64_temp_file)
                os.remove(ref_fpath)
            except Exception as cleanup_error:
                print(f'Cleanup warning: {cleanup_error}')
                
        except Exception as e:
            import traceback
            tb = traceback.format_exc()
            print(f'Analysis error: {e}\n{tb}')
            compare_result = {'status': 'error', 'message': f'analyze_error: {str(e)}', 'traceback': tb}
    else:
        print('No reference image provided!')
        compare_result = {'status': 'error', 'message': 'no_reference_image_provided'}

    return web.json_response({'status': 'ok', 'path': str(fpath.name), 'compare': compare_result})


def main():
    app = web.Application()
    # initialize collections before starting the app to avoid modifying state later
    app['sockets'] = {}
    app['cancel_events'] = {}
    # store pending card reads per websocket while waiting for picture confirmation
    app['pending_cards'] = {}
    app.router.add_get('/', index)
    app.router.add_post('/capture', capture_handler)
    app.router.add_get('/ws', websocket_handler)
    app.router.add_static('/static/', path=ROOT / 'static', show_index=False)

    # Try to enable SSL if cert/key are provided via env or found under static/certs
    ssl_context = None
    cert_path = os.environ.get('SSL_CERT')
    key_path = os.environ.get('SSL_KEY')
    # fallback: check for certs/static/certs/server.crt / server.key
    if not cert_path or not key_path:
        cert_dir = ROOT / 'static' / 'certs'
        pcrt = cert_dir / 'server.crt'
        pkey = cert_dir / 'server.key'
        if pcrt.exists() and pkey.exists():
            cert_path = str(pcrt)
            key_path = str(pkey)

    if cert_path and key_path:
        try:
            ssl_context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
            ssl_context.load_cert_chain(certfile=cert_path, keyfile=key_path)
            print(f"Starting HTTPS server on https://0.0.0.0:8443 using cert {cert_path}")
            web.run_app(app, host='0.0.0.0', port=8443, ssl_context=ssl_context)
            return
        except Exception as e:
            print('Failed to start HTTPS server, falling back to HTTP:', e)

    print('Starting HTTP server on http://0.0.0.0:8080 (no SSL)')
    web.run_app(app, host='0.0.0.0', port=8080)


if __name__ == '__main__':
    main()
