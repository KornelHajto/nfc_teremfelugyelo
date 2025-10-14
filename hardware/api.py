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


def read_card_data(pn532, uid, block=4):
    try:
        key = b'\xFF\xFF\xFF\xFF\xFF\xFF'
        if pn532.mifare_classic_authenticate_block(uid, block, 0x60, key):
            data = pn532.mifare_classic_read_block(block)
            return data.decode('utf-8', errors='ignore').rstrip('\x00')
        return None
    except Exception:
        return None


def init_readers():
    """Initialize and return (pn532_i2c, pn532_uart)."""
    i2c = busio.I2C(board.SCL, board.SDA)
    pn532_i2c = PN532_I2C(i2c)
    pn532_i2c.SAM_configuration()

    uart = serial.Serial("/dev/serial0", baudrate=115200, timeout=1)
    pn532_uart = PN532_UART(uart)
    pn532_uart.SAM_configuration()

    return pn532_i2c, pn532_uart


def detect_card_once(pn532_i2c, pn532_uart, timeout=10.0):
    """Wait up to `timeout` seconds for a card on either reader.

    Returns a tuple (sensor, uid_hex, data) or (None, None, None) on timeout.
    sensor is 'I2C' or 'UART'.
    """
    deadline = time.time() + timeout
    last_uid_i2c = None
    last_uid_uart = None

    while time.time() < deadline:
        uid_i2c = pn532_i2c.read_passive_target(timeout=0.5)
        if uid_i2c is not None and uid_i2c != last_uid_i2c:
            uid_hex = ''.join([format(i, '02X') for i in uid_i2c])
            data = read_card_data(pn532_i2c, uid_i2c)
            return 'I2C', uid_hex, data

        uid_uart = pn532_uart.read_passive_target(timeout=0.5)
        if uid_uart is not None and uid_uart != last_uid_uart:
            uid_hex = ''.join([format(i, '02X') for i in uid_uart])
            data = read_card_data(pn532_uart, uid_uart)
            return 'UART', uid_hex, data

        time.sleep(0.1)

    return None, None, None


# Global storage for the last 'enter' card
last_entry = None

# Default endpoints (change these to your real endpoints)
<<<<<<< HEAD
ENTER_URL = 'http://example.com/enter'
LEAVE_URL = 'http://example.com/leave'
=======
ENTER_URL = 'http://192.168.153.78:5189/api/Keys/enter'
LEAVE_URL = 'http://192.168.153.78:5189/api/leave'
>>>>>>> 288cc85 (working hard on hardware side - batmaan)


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


def enter_mode(timeout=10.0):
    """Wait for a card once, store which sensor it came from and send to ENTER_URL."""
    global last_entry
    print("Enter mode: waiting for a card...")
<<<<<<< HEAD
    pn532_i2c, pn532_uart = init_readers()
    sensor, uid, data = detect_card_once(pn532_i2c, pn532_uart, timeout=timeout)
    if sensor is None:
        print("No card detected (timeout).")
        return

    if sensor == "UART":
        sensor = "Room 2"
    elif sensor == "I2C":
        sensor = "Room 1"

    last_entry = {
        'Room': sensor,
        'Key': data,
    }

    print(last_entry)
=======
    try:
        pn532_i2c, pn532_uart = init_readers()
    except Exception as e:
        return {'status': 'error', 'error': 'init_readers_failed', 'message': str(e)}

    sensor, uid, data = detect_card_once(pn532_i2c, pn532_uart, timeout=timeout)
    if sensor is None:
        return {'status': 'timeout', 'message': 'no_card_detected'}

    if sensor == "UART":
        sensor_name = "Room 2"
    elif sensor == "I2C":
        sensor_name = "Room 1"
    else:
        sensor_name = sensor

    last_entry = {
        'Room': sensor_name,
        'hash': data,
    }

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
>>>>>>> 288cc85 (working hard on hardware side - batmaan)

def leave_mode(timeout=10.0):
    """Wait for a card once in leave mode and send to LEAVE_URL. Includes last_entry if present."""
    print("Leave mode: waiting for a card...")
<<<<<<< HEAD
    pn532_i2c, pn532_uart = init_readers()
    sensor, uid, data = detect_card_once(pn532_i2c, pn532_uart, timeout=timeout)
    if sensor is None:
        print("No card detected (timeout).")
        return

    if sensor == "UART":
        sensor = "Room 2"
    elif sensor == "I2C":
        sensor = "Room 1"

    leave_card = {'Key': data}
    print(leave_card)
=======
    try:
        pn532_i2c, pn532_uart = init_readers()
    except Exception as e:
        return {'status': 'error', 'error': 'init_readers_failed', 'message': str(e)}

    sensor, uid, data = detect_card_once(pn532_i2c, pn532_uart, timeout=timeout)
    if sensor is None:
        return {'status': 'timeout', 'message': 'no_card_detected'}

    if sensor == "UART":
        sensor_name = "Room 2"
    elif sensor == "I2C":
        sensor_name = "Room 1"
    else:
        sensor_name = sensor

    leave_card = {'Room': sensor_name, 'Key': data}

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
>>>>>>> 288cc85 (working hard on hardware side - batmaan)


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
            print(f'last_entry = {last_entry}')
        elif choice == 'q':
            print('Exiting.')
            break
        else:
            print('Invalid choice.')


if __name__ == '__main__':
    mode_menu()
