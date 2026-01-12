#!/usr/bin/env python3
"""
LabKeyEncrypter CLI - Encrypt and decrypt JSON file values.

Usage:
    python main.py <encrypt|decrypt> <file> <password>
"""

import sys
from pathlib import Path
from cryptography.exceptions import InvalidTag
from lab_key_json_file_value_encrypter import encrypt_json_values, decrypt_json_values


def main() -> None:
    if len(sys.argv) != 4:
        print("Usage: python main.py <encrypt|decrypt> <file> <password>")
        sys.exit(1)
    
    operation = sys.argv[1]
    file_path = sys.argv[2]
    password = sys.argv[3]
    
    if operation == "encrypt":
        try:
            encrypt_json_values(file_path, password)
        except FileNotFoundError as ex:
            print(f"Error: {ex}")
            sys.exit(1)
    
    elif operation == "decrypt":
        try:
            decrypt_json_values(file_path, password)
        except (ValueError, InvalidTag) as ex:
            print(f"Error decrypting value: {ex}")
            sys.exit(1)
        except FileNotFoundError as ex:
            print(f"Error: {ex}")
            sys.exit(1)
    
    else:
        print(f"Invalid operation: {operation}")
        sys.exit(1)


if __name__ == "__main__":
    main()
