# LabKeyEncrypter - Python

A Python implementation of the LabKeyEncrypter, compatible with the C# version.

## Features

- AES-256-CBC encryption with PBKDF2-SHA1 key derivation
- Cross-platform compatibility with the C# LabKeyEncrypter
- Encrypt/decrypt individual strings or JSON file values

## Installation

```bash
pip install -r requirements.txt
```

## Usage

### Command Line

```bash
# Encrypt all values in a JSON file
python main.py encrypt input.json password

# Decrypt all values in an encrypted JSON file
python main.py decrypt input_encrypted.json password
```

### As a Library

```python
from lab_key_encrypter import encrypt, decrypt

# Encrypt a string
encrypted = encrypt("my secret", "password")

# Decrypt a string
decrypted = decrypt(encrypted, "password")
```

### JSON File Encryption

```python
from lab_key_json_file_value_encrypter import encrypt_json_values, decrypt_json_values

# Encrypt all values in a JSON file
encrypt_json_values("config.json", "password")
# Creates config_encrypted.json

# Decrypt all values in an encrypted JSON file
decrypt_json_values("config_encrypted.json", "password")
# Creates config.json
```

## Running Tests

```bash
pytest test_lab_key_encrypter.py -v
```

## Compatibility

This implementation is fully compatible with the C# LabKeyEncrypter. Data encrypted with one can be decrypted by the other.
