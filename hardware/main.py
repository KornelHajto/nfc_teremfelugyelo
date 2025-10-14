import board
import busio
import serial
import time
from adafruit_pn532.i2c import PN532_I2C
from adafruit_pn532.uart import PN532_UART

# --- Room 1: I2C ---
i2c1 = busio.I2C(board.SCL, board.SDA)
pn_room1 = PN532_I2C(i2c1)
pn_room1.SAM_configuration()

# --- Room 2: UART ---
uart = serial.Serial("/dev/serial0", baudrate=115200, timeout=1)
pn_room2 = PN532_UART(uart)
pn_room2.SAM_configuration()

# Keep track of last seen UIDs to avoid repeated prints
last_uid_room1 = None
last_uid_room2 = None

print("Waiting for NFC tags...")

while True:
    # --- Room 1 ---
    uid1 = pn_room1.read_passive_target(timeout=0.2)
    if uid1 != last_uid_room1 and uid1 is not None:
        print("Room 1 tag UID:", [hex(i) for i in uid1])
        last_uid_room1 = uid1
    elif uid1 is None:
        last_uid_room1 = None  # reset when tag removed

    # --- Room 2 ---
    uid2 = pn_room2.read_passive_target(timeout=0.2)
    if uid2 != last_uid_room2 and uid2 is not None:
        print("Room 2 tag UID:", [hex(i) for i in uid2])
        last_uid_room2 = uid2
    elif uid2 is None:
        last_uid_room2 = None  # reset when tag removed

    time.sleep(0.1)
