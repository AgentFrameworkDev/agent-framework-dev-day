"""
LabKeyEncrypter - AES-256 encryption with PBKDF2 key derivation.

Compatible with the C# LabKeyEncrypter implementation.
"""

import os
import base64
import hashlib
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives import padding
from cryptography.hazmat.backends import default_backend

SALT_SIZE = 16  # 128 bit
KEY_SIZE = 32   # 256 bit
IV_SIZE = 16    # 128 bit
ITERATIONS = 10000


def _generate_salt() -> bytes:
    """Generate a cryptographically secure random salt."""
    return os.urandom(SALT_SIZE)


def _derive_key(password: str, salt: bytes) -> bytes:
    """Derive an encryption key from password using PBKDF2-SHA1."""
    return hashlib.pbkdf2_hmac('sha1', password.encode('utf-8'), salt, ITERATIONS, KEY_SIZE)


def encrypt(plain_text: str, password: str) -> str:
    """
    Encrypt plain text using AES-256-CBC with PBKDF2 key derivation.
    
    Args:
        plain_text: The text to encrypt.
        password: The password to derive the encryption key from.
    
    Returns:
        Base64-encoded string containing salt + IV + ciphertext.
    """
    salt = _generate_salt()
    key = _derive_key(password, salt)
    iv = os.urandom(IV_SIZE)
    
    # Pad the plaintext to block size
    padder = padding.PKCS7(128).padder()
    padded_data = padder.update(plain_text.encode('utf-8')) + padder.finalize()
    
    # Encrypt
    cipher = Cipher(algorithms.AES(key), modes.CBC(iv), backend=default_backend())
    encryptor = cipher.encryptor()
    encrypted = encryptor.update(padded_data) + encryptor.finalize()
    
    # Combine salt + iv + encrypted
    result = salt + iv + encrypted
    
    return base64.b64encode(result).decode('utf-8')


def decrypt(cipher_text: str, password: str) -> str:
    """
    Decrypt cipher text that was encrypted with the encrypt function.
    
    Args:
        cipher_text: Base64-encoded string containing salt + IV + ciphertext.
        password: The password used for encryption.
    
    Returns:
        The decrypted plain text.
    """
    full_cipher = base64.b64decode(cipher_text)
    
    # Extract salt, IV, and cipher
    salt = full_cipher[:SALT_SIZE]
    iv = full_cipher[SALT_SIZE:SALT_SIZE + IV_SIZE]
    encrypted = full_cipher[SALT_SIZE + IV_SIZE:]
    
    key = _derive_key(password, salt)
    
    # Decrypt
    cipher = Cipher(algorithms.AES(key), modes.CBC(iv), backend=default_backend())
    decryptor = cipher.decryptor()
    padded_data = decryptor.update(encrypted) + decryptor.finalize()
    
    # Unpad
    unpadder = padding.PKCS7(128).unpadder()
    plain_data = unpadder.update(padded_data) + unpadder.finalize()
    
    return plain_data.decode('utf-8')
