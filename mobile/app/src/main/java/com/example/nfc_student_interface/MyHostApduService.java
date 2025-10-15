package com.example.nfc_student_interface;

import android.nfc.cardemulation.HostApduService;
import android.os.Bundle;

import java.nio.charset.StandardCharsets;

public class MyHostApduService extends HostApduService {

    private static final byte[] SELECT_OK_SW = {(byte)0x90, 0x00};
    private static final byte[] UNKNOWN_CMD_SW = {(byte)0x00, (byte)0x00};

    // A field to hold your 16-character payload, in bytes
    public static final byte[] MY_PAYLOAD = "1234567890ABCDEF".getBytes(StandardCharsets.UTF_8);
    public byte[] myPayload;

    @Override
    public void onCreate() {
        super.onCreate();
        // initialize default payload, or leave null until button pressed
        myPayload = "1234567890ABCDEF".getBytes(StandardCharsets.UTF_8);
    }

    public byte[] processCommandApdu(byte[] commandApdu, Bundle extras) {
        // Check if the APDU is a SELECT command for your AID
        if (isSelectAidApdu(commandApdu)) {
            return SELECT_OK_SW; // Return success status word
        }

        // Check if the APDU is a READ BINARY command
        if (isReadBinaryApdu(commandApdu)) {
            if (MY_PAYLOAD != null) {
                // Return the payload followed by the success status word
                return concat(MY_PAYLOAD, SELECT_OK_SW);
            } else {
                // Return error status word if payload is null
                return UNKNOWN_CMD_SW;
            }
        }

        // Return error status word for unrecognized commands
        return UNKNOWN_CMD_SW;
    }

    @Override
    public void onDeactivated(int reason) {
        // Called when connection to reader is lost (e.g. user moves away)
    }

    private boolean isSelectAidApdu(byte[] apdu) {
        // Check if the APDU is a SELECT command for your AID
        return apdu.length > 5 && apdu[0] == (byte) 0x00 && apdu[1] == (byte) 0xA4
                && apdu[2] == (byte) 0x04 && apdu[3] == (byte) 0x00;
    }

    private boolean isReadBinaryApdu(byte[] apdu) {
        // Check if the APDU is a READ BINARY command
        return apdu.length == 5 && apdu[1] == (byte) 0xB0;
    }

    private byte[] concat(byte[] a, byte[] b) {
        byte[] result = new byte[a.length + b.length];
        System.arraycopy(a, 0, result, 0, a.length);
        System.arraycopy(b, 0, result, a.length, b.length);
        return result;
    }
}

