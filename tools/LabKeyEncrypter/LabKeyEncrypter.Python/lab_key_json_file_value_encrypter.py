"""
LabKeyJsonFileValueEncrypter - Encrypt/decrypt JSON file values.

Compatible with the C# LabKeyJsonFileValueEncrypter implementation.
"""

import json
from pathlib import Path
from lab_key_encrypter import encrypt, decrypt


def encrypt_json_values(file_path: str, password: str) -> None:
    """
    Encrypt all values in a JSON file and save to a new file.
    
    Args:
        file_path: Path to the JSON file to encrypt.
        password: The password to use for encryption.
    """
    path = Path(file_path)
    json_string = path.read_text(encoding='utf-8')
    json_object = json.loads(json_string)
    
    encrypted_object = {}
    for key, value in json_object.items():
        plain_text = json.dumps(value) if not isinstance(value, str) else value
        encrypted_object[key] = encrypt(plain_text, password)
    
    encrypted_file_path = str(path).replace('.json', '_encrypted.json')
    Path(encrypted_file_path).write_text(
        json.dumps(encrypted_object, indent=2),
        encoding='utf-8'
    )


def decrypt_json_values(file_path: str, password: str) -> bool:
    """
    Decrypt all values in an encrypted JSON file and save to a new file.
    
    Args:
        file_path: Path to the encrypted JSON file.
        password: The password used for encryption.
    
    Returns:
        True if decryption was successful.
    """
    path = Path(file_path)
    json_string = path.read_text(encoding='utf-8')
    json_object = json.loads(json_string)
    
    decrypted_object = {}
    for key, value in json_object.items():
        decrypted_text = decrypt(value, password)
        # Try to parse as JSON, otherwise use as string
        try:
            decrypted_object[key] = json.loads(decrypted_text)
        except json.JSONDecodeError:
            decrypted_object[key] = decrypted_text
    
    decrypted_file_path = str(path).replace('_encrypted.json', '.json')
    Path(decrypted_file_path).write_text(
        json.dumps(decrypted_object, indent=2),
        encoding='utf-8'
    )
    
    return True
