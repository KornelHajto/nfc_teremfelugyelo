#!/usr/bin/env python3
"""
Functions to reconstruct an image from a base64 encoded string.
Can be imported as a module or run standalone.
"""

import base64
import os


def reconstruct_image_from_base64_string(b64_string, output_path):
    """
    Reconstruct an image from a base64 encoded string.
    
    Args:
        b64_string: Base64 encoded string (can include data URL prefix)
        output_path: Path where the image should be saved
        
    Returns:
        tuple: (success: bool, error_message: str or None)
    """
    try:
        # Clean the string
        b64_clean = b64_string.strip()
        
        # Remove data URL prefix if present
        if "," in b64_clean:
            b64_clean = b64_clean.split(",", 1)[1]
        
        # Remove any whitespace, newlines, tabs
        b64_clean = b64_clean.replace('\n', '').replace('\r', '').replace(' ', '').replace('\t', '')
        
        # Add padding if needed for proper base64 decoding
        missing_padding = len(b64_clean) % 4
        if missing_padding:
            b64_clean += '=' * (4 - missing_padding)
        
        # Decode the base64 string
        image_bytes = base64.b64decode(b64_clean)
        
        # Save to file
        with open(output_path, "wb") as f:
            f.write(image_bytes)
        
        # Verify file was created
        if not os.path.exists(output_path):
            return False, "File was not created"
        
        file_size = os.path.getsize(output_path)
        if file_size == 0:
            return False, "Created file is empty"
        
        # Verify with OpenCV if available
        try:
            import cv2
            img = cv2.imread(output_path)
            if img is None:
                return False, "OpenCV couldn't read the file as an image"
        except ImportError:
            pass  # OpenCV not available, skip verification
        
        return True, None
        
    except base64.binascii.Error as e:
        return False, f"Base64 decoding error: {e}"
    except Exception as e:
        return False, f"Error: {e}"


def reconstruct_image_from_file(input_file_path, output_image_path):
    """
    Read base64 string from a file and reconstruct it as an image.
    
    Args:
        input_file_path: Path to file containing base64 string
        output_image_path: Path where the reconstructed image will be saved
    
    Returns:
        tuple: (success: bool, error_message: str or None)
    """
    try:
        with open(input_file_path, 'r') as f:
            b64_string = f.read()
        return reconstruct_image_from_base64_string(b64_string, output_image_path)
    except Exception as e:
        return False, f"Failed to read input file: {e}"


if __name__ == "__main__":
    import sys
    
    # Command line mode: python3 reconstruct_image.py input.txt output.jpg
    if len(sys.argv) > 1:
        input_file = sys.argv[1]
        output_file = sys.argv[2] if len(sys.argv) > 2 else "decoded_image.jpg"
        
        print(f"Reading base64 from: {input_file}")
        print(f"Output image: {output_file}")
        
        success, error = reconstruct_image_from_file(input_file, output_file)
        
        if success:
            print(f"✅ Image saved as: {output_file}")
        else:
            print(f"❌ Failed: {error}")
            sys.exit(1)
    else:
        # Default hardcoded mode
        input_file = "message.txt"
        output_file = "decoded_image.png"
        
        success, error = reconstruct_image_from_file(input_file, output_file)
        
        if success:
            print(f"✅ Image saved as: {output_file}")
        else:
            print(f"❌ Failed: {error}")
