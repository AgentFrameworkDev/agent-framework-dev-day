"""
LabKeyEncrypter - AES-256-GCM encryption with PBKDF2-SHA256 key derivation.

Compatible with the C# LabKeyEncrypter implementation.
Compliant with OWASP cryptographic storage guidelines.
"""

import os
import base64
import hashlib
from cryptography.hazmat.primitives.ciphers.aead import AESGCM

SALT_SIZE = 16      # 128 bit
KEY_SIZE = 32       # 256 bit
NONCE_SIZE = 12     # 96 bit (recommended for AES-GCM)
TAG_SIZE = 16       # 128 bit (AES-GCM authentication tag, appended to ciphertext by AESGCM)
ITERATIONS = 600000 # OWASP recommended minimum for PBKDF2-SHA256


def _generate_salt() -> bytes:
    """Generate a cryptographically secure random salt."""
    return os.urandom(SALT_SIZE)


def _derive_key(password: str, salt: bytes) -> bytes:
    """Derive an encryption key from password using PBKDF2-SHA256."""
    return hashlib.pbkdf2_hmac('sha256', password.encode('utf-8'), salt, ITERATIONS, KEY_SIZE)


def _clear_bytes(data: bytearray) -> None:
    """Securely clear sensitive data from memory."""
    for i in range(len(data)):
        data[i] = 0


def encrypt(plain_text: str, password: str) -> str:
    """
    Encrypt plain text using AES-256-GCM with PBKDF2-SHA256 key derivation.
    
    Args:
        plain_text: The text to encrypt.
        password: The password to derive the encryption key from.
    
    Returns:
        Base64-encoded string containing salt + nonce + tag + ciphertext.
    """
    salt = _generate_salt()
    key = bytearray(_derive_key(password, salt))
    nonce = os.urandom(NONCE_SIZE)
    
    # Encrypt using AES-GCM (authentication tag is appended to ciphertext)
    aesgcm = AESGCM(bytes(key))
    ciphertext_with_tag = aesgcm.encrypt(nonce, plain_text.encode('utf-8'), None)
    
    # AESGCM appends the 16-byte tag to the ciphertext
    # Extract tag and ciphertext separately for C# compatibility
    ciphertext = ciphertext_with_tag[:-TAG_SIZE]
    tag = ciphertext_with_tag[-TAG_SIZE:]
    
    # Clear the key from memory
    _clear_bytes(key)
    
    # Format: salt (16) + nonce (12) + tag (16) + ciphertext
    result = salt + nonce + tag + ciphertext
    
    return base64.b64encode(result).decode('utf-8')


def decrypt(cipher_text: str, password: str) -> str:
    """
    Decrypt cipher text that was encrypted with the encrypt function.
    
    Args:
        cipher_text: Base64-encoded string containing salt + nonce + tag + ciphertext.
        password: The password used for encryption.
    
    Returns:
        The decrypted plain text.
    
    Raises:
        cryptography.exceptions.InvalidTag: If authentication fails (tampered data or wrong password).
    """
    full_cipher = base64.b64decode(cipher_text)
    
    # Extract components: salt (16) + nonce (12) + tag (16) + ciphertext
    salt = full_cipher[:SALT_SIZE]
    nonce = full_cipher[SALT_SIZE:SALT_SIZE + NONCE_SIZE]
    tag = full_cipher[SALT_SIZE + NONCE_SIZE:SALT_SIZE + NONCE_SIZE + TAG_SIZE]
    ciphertext = full_cipher[SALT_SIZE + NONCE_SIZE + TAG_SIZE:]
    
    key = bytearray(_derive_key(password, salt))
    
    # Reconstruct ciphertext with tag appended (as expected by AESGCM)
    ciphertext_with_tag = ciphertext + tag
    
    # Decrypt using AES-GCM
    aesgcm = AESGCM(bytes(key))
    plain_bytes = aesgcm.decrypt(nonce, ciphertext_with_tag, None)
    
    # Clear the key from memory
    _clear_bytes(key)
    
    return plain_bytes.decode('utf-8')
