#!/usr/bin/env python3
import board
import busio
import serial
import time
from adafruit_pn532.i2c import PN532_I2C
from adafruit_pn532.uart import PN532_UART

def read_card_data(pn532, uid, block=4):
    try:
        key = b'\xFF\xFF\xFF\xFF\xFF\xFF'
        if pn532.mifare_classic_authenticate_block(uid, block, 0x60, key):
            data = pn532.mifare_classic_read_block(block)
            return data.decode('utf-8', errors='ignore').rstrip('\x00')
        return None
    except:
        return None

def main():
    print("Initializing readers...")
    i2c = busio.I2C(board.SCL, board.SDA)
    pn532_i2c = PN532_I2C(i2c)
    pn532_i2c.SAM_configuration()
    
    uart = serial.Serial("/dev/serial0", baudrate=115200, timeout=1)
    pn532_uart = PN532_UART(uart)
    pn532_uart.SAM_configuration()
    
    print("Ready. Waiting for cards...\n")
    
    last_uid_i2c = None
    last_uid_uart = None
    
    try:
        while True:
            uid_i2c = pn532_i2c.read_passive_target(timeout=0.2)
            
            if uid_i2c is not None and uid_i2c != last_uid_i2c:
                uid_hex = ''.join([format(i, '02X') for i in uid_i2c])
                data = read_card_data(pn532_i2c, uid_i2c)
                print(f"I2C   | Card: {uid_hex} | Data: {data if data else '(none)'}")
                last_uid_i2c = uid_i2c
            elif uid_i2c is None:
                last_uid_i2c = None
            
            uid_uart = pn532_uart.read_passive_target(timeout=0.2)
            
            if uid_uart is not None and uid_uart != last_uid_uart:
                uid_hex = ''.join([format(i, '02X') for i in uid_uart])
                data = read_card_data(pn532_uart, uid_uart)
                print(f"UART  | Card: {uid_hex} | Data: {data if data else '(none)'}")
                last_uid_uart = uid_uart
            elif uid_uart is None:
                last_uid_uart = None
            
            time.sleep(0.1)
            
    except KeyboardInterrupt:
        print("\n\nExiting...")

if __name__ == '__main__':
    main()
