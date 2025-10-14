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

    async for msg in ws:
        if msg.type == web.WSMsgType.TEXT:
            text = msg.data.strip().lower()
            if text == 'enter':
                # Run blocking enter_mode in executor
                await ws.send_str('Waiting for card in enter mode...')
                result = await loop.run_in_executor(None, api.enter_mode)
                # result is a dict with status
                if not isinstance(result, dict):
                    await ws.send_str(json.dumps({'event': 'enter_done', 'last_entry': api.last_entry}))
                else:
                    if result.get('status') == 'ok':
                        await ws.send_str(json.dumps({'event': 'enter_done', 'result': result, 'last_entry': api.last_entry}))
                    else:
                        await ws.send_str(json.dumps({'event': 'enter_error', 'result': result, 'last_entry': api.last_entry}))

            elif text == 'leave':
                await ws.send_str('Waiting for card in leave mode...')
                result = await loop.run_in_executor(None, api.leave_mode)
                if not isinstance(result, dict):
                    await ws.send_str(json.dumps({'event': 'leave_done', 'last_entry': api.last_entry}))
                else:
                    if result.get('status') == 'ok':
                        await ws.send_str(json.dumps({'event': 'leave_done', 'result': result, 'last_entry': api.last_entry}))
                    else:
                        await ws.send_str(json.dumps({'event': 'leave_error', 'result': result, 'last_entry': api.last_entry}))

            elif text == 'get_last':
                await ws.send_str(json.dumps({'event': 'last_entry', 'last_entry': api.last_entry}))

            else:
                await ws.send_str('Unknown command')

        elif msg.type == web.WSMsgType.ERROR:
            print('ws connection closed with exception %s' % ws.exception())

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
