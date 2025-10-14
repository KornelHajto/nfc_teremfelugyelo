#!/usr/bin/env python3
import asyncio
import json
import pathlib
from aiohttp import web

import api

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
    # detect room parameter from the ws request URL (e.g. /ws?room=room1)
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
            # commands: 'enter', 'enter room1', 'leave', 'leave room2', 'get_last'
            if cmd[0] == 'enter':
                # use explicit room argument or fallback to the websocket's registered room
                room = None
                if len(cmd) > 1:
                    room = cmd[1]
                if room is None:
                    room = conn_room
                await ws.send_str('Waiting for card in enter mode...')
                # run enter_mode in executor and pass this ws's cancel event
                cancel_ev = app['cancel_events'].get(ws)
                def call_enter():
                    return api.enter_mode(room, timeout=30.0, cancel_event=cancel_ev)
                result = await loop.run_in_executor(None, call_enter)
                # broadcast result only to sockets registered for the same room
                if result.get('status') == 'ok':
                    event = 'enter_done'
                elif result.get('status') == 'cancelled':
                    event = 'enter_cancelled'
                elif result.get('status') == 'timeout':
                    event = 'enter_timeout'
                else:
                    event = 'enter_error'
                payload = json.dumps({'event': event, 'result': result, 'last_entry': api.last_entry})
                # determine room for this result
                res_room = None
                if isinstance(result, dict):
                    if 'entry' in result and isinstance(result['entry'], dict):
                        res_room = result['entry'].get('room')
                    elif 'leave' in result and isinstance(result['leave'], dict):
                        res_room = result['leave'].get('room')
                # if the result has no room, send only to the initiating websocket
                # strict per-room broadcast: only send to sockets whose registered room matches res_room
                if res_room is None:
                    try:
                        await ws.send_str(payload)
                    except Exception:
                        app['sockets'].pop(ws, None)
                else:
                    for s, s_room in list(app['sockets'].items()):
                        try:
                            if s_room == res_room:
                                await s.send_str(payload)
                        except Exception:
                            app['sockets'].pop(s, None)

            elif cmd[0] == 'leave':
                room = None
                if len(cmd) > 1:
                    room = cmd[1]
                if room is None:
                    room = conn_room
                await ws.send_str('Waiting for card in leave mode...')
                cancel_ev = app['cancel_events'].get(ws)
                def call_leave():
                    return api.leave_mode(room, timeout=30.0, cancel_event=cancel_ev)
                result = await loop.run_in_executor(None, call_leave)
                if result.get('status') == 'ok':
                    event = 'leave_done'
                elif result.get('status') == 'cancelled':
                    event = 'leave_cancelled'
                elif result.get('status') == 'timeout':
                    event = 'leave_timeout'
                else:
                    event = 'leave_error'
                payload = json.dumps({'event': event, 'result': result, 'last_entry': api.last_entry})
                res_room = None
                if isinstance(result, dict):
                    if 'entry' in result and isinstance(result['entry'], dict):
                        res_room = result['entry'].get('room')
                    elif 'leave' in result and isinstance(result['leave'], dict):
                        res_room = result['leave'].get('room')
                if res_room is None:
                    try:
                        await ws.send_str(payload)
                    except Exception:
                        app['sockets'].pop(ws, None)
                else:
                    for s, s_room in list(app['sockets'].items()):
                        try:
                            if s_room == res_room:
                                await s.send_str(payload)
                        except Exception:
                            app['sockets'].pop(s, None)

            elif cmd[0] == 'get_last':
                # return the last entry only for this websocket's room
                key = conn_room or 'unknown'
                last = api.last_entries.get(key)
                await ws.send_str(json.dumps({'event': 'last_entry', 'last_entry': last}))
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


def main():
    app = web.Application()
    app.router.add_get('/', index)
    app.router.add_get('/ws', websocket_handler)
    app.router.add_static('/static/', path=ROOT / 'static', show_index=False)

    web.run_app(app, host='0.0.0.0', port=8080)


if __name__ == '__main__':
    main()
