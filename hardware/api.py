#!/usr/bin/env python3
import board
import busio
import serial
import time
from adafruit_pn532.i2c import PN532_I2C
from adafruit_pn532.uart import PN532_UART

try:
    import requests
except Exception:
    requests = None


def read_hce_data(pn532, uid):
    """Try to read data from HCE (Host Card Emulation) device like Android phone.
    
    Uses ISO-DEP (ISO 14443-4) protocol to communicate with HCE apps.
    Returns the data string or None if failed.
    """
    try:
        # SELECT APDU command for custom AID
        select_apdu = bytes.fromhex('00A4040007F0010203040506')
        
        # Try to communicate using InDataExchange
        response = pn532.call_function(
            0x40,  # InDataExchange command
            params=bytearray([0x01]) + bytearray(select_apdu),
            response_length=255,
            timeout=1.0
        )
        
        if response and len(response) > 1:
            # Check status byte (first byte should be 0x00 for success)
            status = response[0]
            if status == 0x00:
                # Extract data (skip status byte)
                data_bytes = response[1:]
                # Remove status word (last 2 bytes like 0x90 0x00)
                if len(data_bytes) >= 2:
                    data_bytes = data_bytes[:-2]
                # Convert to string
                data = data_bytes.decode('utf-8', errors='ignore').rstrip('\x00')
                return data if data else None
        return None
    except Exception as e:
        print(f"HCE read error: {e}")
        return None


def read_card_data(pn532, uid, block=4):
    """Try to read data from MIFARE Classic card.
    
    Returns the data string or None if failed.
    """
    try:
        key = b'\xFF\xFF\xFF\xFF\xFF\xFF'
        if pn532.mifare_classic_authenticate_block(uid, block, 0x60, key):
            data = pn532.mifare_classic_read_block(block)
            return data.decode('utf-8', errors='ignore').rstrip('\x00')
        return None
    except Exception:
        return None


def read_card_or_hce(pn532, uid):
    """Try to read from both MIFARE Classic and HCE.
    
    First tries MIFARE Classic, then HCE if that fails.
    Returns the data string or None.
    """
    # Try MIFARE Classic first
    data = read_card_data(pn532, uid)
    if data:
        return data
    
    # If MIFARE fails, try HCE
    data = read_hce_data(pn532, uid)
    return data


def init_readers():
    """Initialize and return (pn532_i2c, pn532_uart)."""
    # default: initialize both readers
    i2c = busio.I2C(board.SCL, board.SDA)
    pn532_i2c = PN532_I2C(i2c)
    pn532_i2c.SAM_configuration()

    uart = serial.Serial("/dev/serial0", baudrate=115200, timeout=1)
    pn532_uart = PN532_UART(uart)
    pn532_uart.SAM_configuration()

    return pn532_i2c, pn532_uart


def init_readers_for(room=None):
    """Initialize only the readers needed for `room`.

    room=None -> both, 'pc0(PC0)' -> only I2C, 'pc1(PC1)' -> only UART
    Returns (pn532_i2c_or_None, pn532_uart_or_None)
    """
    pn532_i2c = None
    pn532_uart = None
    if room is None or room == 'pc0':
        i2c = busio.I2C(board.SCL, board.SDA)
        pn532_i2c = PN532_I2C(i2c)
        pn532_i2c.SAM_configuration()
    if room is None or room == 'pc1':
        uart = serial.Serial("/dev/serial0", baudrate=115200, timeout=1)
        pn532_uart = PN532_UART(uart)
        pn532_uart.SAM_configuration()
    return pn532_i2c, pn532_uart


def detect_card_once(pn532_i2c, pn532_uart, timeout=10.0):
    """Wait up to `timeout` seconds for a card on either reader.

    Returns a tuple (sensor, uid_hex, data) or (None, None, None) on timeout.
    sensor is 'I2C' or 'UART'.
    Supports both MIFARE Classic cards and HCE devices.
    """
    deadline = time.time() + timeout
    last_uid_i2c = None
    last_uid_uart = None

    while time.time() < deadline:
        uid_i2c = pn532_i2c.read_passive_target(timeout=0.5)
        if uid_i2c is not None and uid_i2c != last_uid_i2c:
            uid_hex = ''.join([format(i, '02X') for i in uid_i2c])
            data = read_card_or_hce(pn532_i2c, uid_i2c)
            return 'I2C', uid_hex, data

        uid_uart = pn532_uart.read_passive_target(timeout=0.5)
        if uid_uart is not None and uid_uart != last_uid_uart:
            uid_hex = ''.join([format(i, '02X') for i in uid_uart])
            data = read_card_or_hce(pn532_uart, uid_uart)
            return 'UART', uid_hex, data

        time.sleep(0.1)

    return None, None, None


def detect_card_for_room(pn532_i2c, pn532_uart, room=None, timeout=10.0, cancel_event=None):
    """Wait up to `timeout` seconds for a card on the reader for `room`.

    room: None -> any (original behavior), 'pc0' -> I2C only, 'pc1' -> UART only
    Returns (sensor, uid_hex, data) or (None, None, None) on timeout.
    """
    deadline = time.time() + timeout
    last_uid_i2c = None
    last_uid_uart = None

    while time.time() < deadline:
        # allow cooperative cancellation from the caller (threading.Event)
        try:
            if cancel_event is not None and getattr(cancel_event, 'is_set', lambda: False)():
                return 'CANCELLED', None, None
        except Exception:
            # if cancel_event is mis-specified, ignore and continue
            pass
        if room is None or room == 'pc0':
            if pn532_i2c is not None:
                try:
                    uid_i2c = pn532_i2c.read_passive_target(timeout=0.5)
                except RuntimeError as e:
                    # transient PN532 error; skip this poll
                    # print('I2C read error:', e)
                    uid_i2c = None
                if uid_i2c is not None and uid_i2c != last_uid_i2c:
                    uid_hex = ''.join([format(i, '02X') for i in uid_i2c])
                    data = read_card_or_hce(pn532_i2c, uid_i2c)
                    return 'I2C', uid_hex, data
                elif uid_i2c is None:
                    last_uid_i2c = None

        if room is None or room == 'pc1':
            if pn532_uart is not None:
                try:
                    uid_uart = pn532_uart.read_passive_target(timeout=0.5)
                except RuntimeError as e:
                    # transient PN532 error on UART reader
                    # print('UART read error:', e)
                    uid_uart = None
                if uid_uart is not None and uid_uart != last_uid_uart:
                    uid_hex = ''.join([format(i, '02X') for i in uid_uart])
                    data = read_card_or_hce(pn532_uart, uid_uart)
                    return 'UART', uid_hex, data
                elif uid_uart is None:
                    last_uid_uart = None

        time.sleep(0.1)

    return None, None, None


# Per-room storage for the last 'enter' card to avoid cross-room leakage
last_entries = {}

# Default endpoints (change these to your real endpoints)
ENTER_URL = 'http://192.168.153.78:5189/api/Keys/enter'
LEAVE_URL = 'http://192.168.153.78:5189/api/Keys/exit'


def send_post(url, payload):
    if requests is None:
        print(f"Requests library not available; would have POSTed to {url} with: {payload}")
        return None
    try:
        r = requests.post(url, json=payload, timeout=5)
        return r
    except Exception as e:
        print(f"HTTP POST error: {e}")
        return None


def enter_mode(room=None, timeout=10.0, cancel_event=None):
    """Wait for a card on the reader assigned to `room` ('pc0' or 'pc1').

    Returns a dict with structured result.
    """
    print(f"Enter mode: waiting for a card... room={room}")
    try:
        pn532_i2c, pn532_uart = init_readers_for(room)
    except Exception as e:
        return {'status': 'error', 'error': 'init_readers_failed', 'message': str(e)}
    try:
        # pass cancel_event through to the detect loop so callers (websocket) can cancel
        sensor, uid, data = detect_card_for_room(pn532_i2c, pn532_uart, room=room, timeout=timeout, cancel_event=cancel_event)
    except Exception as e:
        return {'status': 'error', 'error': 'detection_failed', 'message': str(e)}
    if sensor is None:
        return {'status': 'timeout', 'message': 'no_card_detected'}
    if sensor == 'CANCELLED':
        return {'status': 'cancelled', 'message': 'operation_cancelled'}

    if sensor == "UART":
        sensor_name = "PC1"
        room_name = 'pc1'
    elif sensor == "I2C":
        sensor_name = "PC0"
        room_name = 'pc0'
    else:
        sensor_name = sensor
        room_name = None

    last_entry = {
        'Room': sensor_name,
        'hash': data,
        'uid': uid,
        'room': room_name,
    }
    # store per-room
    key = room_name or 'unknown'
    last_entries[key] = last_entry

    resp = send_post(ENTER_URL, last_entry)
    if resp is None:
        return {'status': 'http_error', 'message': 'no_response', 'entry': last_entry}

    try:
        content = resp.json()
    except Exception:
        content = resp.text

    if 200 <= resp.status_code < 300:
                return {'status': 'ok', 'code': resp.status_code, 'response': content, 'entry': last_entry}
    else:
        return {'status': 'error', 'code': resp.status_code, 'response': content, 'entry': last_entry}

def leave_mode(room=None, timeout=10.0, cancel_event=None):
    """Wait for a card on the reader assigned to `room` and send leave request."""
    print(f"Leave mode: waiting for a card... room={room}")
    try:
        pn532_i2c, pn532_uart = init_readers_for(room)
    except Exception as e:
        return {'status': 'error', 'error': 'init_readers_failed', 'message': str(e)}
    try:
        sensor, uid, data = detect_card_for_room(pn532_i2c, pn532_uart, room=room, timeout=timeout, cancel_event=cancel_event)
    except Exception as e:
        return {'status': 'error', 'error': 'detection_failed', 'message': str(e)}
    if sensor is None:
        return {'status': 'timeout', 'message': 'no_card_detected'}
    if sensor == 'CANCELLED':
        return {'status': 'cancelled', 'message': 'operation_cancelled'}

    if sensor == "UART":
        sensor_name = "PC1"
        room_name = 'pc1'
    elif sensor == "I2C":
        sensor_name = "PC0"
        room_name = 'pc0'
    else:
        sensor_name = sensor
        room_name = None

    leave_card = {'Room': sensor_name, 'Key': data, 'uid': uid, 'room': room_name}

    # store per-room last leave info too (named with 'leave' prefix)
    key = room_name or 'unknown'
    last_entries[key] = leave_card

    resp = send_post(LEAVE_URL, leave_card)
    if resp is None:
        return {'status': 'http_error', 'message': 'no_response', 'leave': leave_card}

    try:
        content = resp.json()
    except Exception:
        content = resp.text

    if 200 <= resp.status_code < 300:
        return {'status': 'ok', 'code': resp.status_code, 'response': content, 'leave': leave_card}
    else:
        return {'status': 'error', 'code': resp.status_code, 'response': content, 'leave': leave_card}


def mode_menu():
    while True:
        print('\nSelect mode:')
        print('1. Enter mode (read one card, save sensor & send)')
        print('2. Leave mode (read one card, send)')
        print('s. Show last entry')
        print('q. Quit')
        choice = input('Your choice: ').strip().lower()
        if choice == '1':
            enter_mode()
        elif choice == '2':
            leave_mode()
        elif choice == 's':
            print('last_entries (per-room):')
            for k, v in last_entries.items():
                print(f'  {k}: {v}')
        elif choice == 'q':
            print('Exiting.')
            break
        else:
            print('Invalid choice.')


if __name__ == '__main__':
    mode_menu()
