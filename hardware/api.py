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
ENTER_URL = 'http://example.com/enter'
LEAVE_URL = 'http://example.com/leave'


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

def leave_mode(timeout=10.0):
    """Wait for a card once in leave mode and send to LEAVE_URL. Includes last_entry if present."""
    print("Leave mode: waiting for a card...")
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
