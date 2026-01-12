"""Unit tests for LabKeyEncrypter."""

import pytest
from lab_key_encrypter import encrypt, decrypt


class TestLabKeyEncrypter:
    """Tests for the encrypt and decrypt functions."""

    def test_encrypt_decrypt_roundtrip(self):
        """Test that encrypting and decrypting returns the original text."""
        plain_text = "This is a secret"
        password = "password"
        
        encrypted = encrypt(plain_text, password)
        decrypted = decrypt(encrypted, password)
        
        assert decrypted == plain_text

    def test_encrypt_produces_different_output_each_time(self):
        """Test that encryption produces different output due to random salt/IV."""
        plain_text = "This is a secret"
        password = "password"
        
        encrypted1 = encrypt(plain_text, password)
        encrypted2 = encrypt(plain_text, password)
        
        assert encrypted1 != encrypted2

    def test_decrypt_with_wrong_password_fails(self):
        """Test that decryption with wrong password fails."""
        plain_text = "This is a secret"
        password = "password"
        wrong_password = "wrongpassword"
        
        encrypted = encrypt(plain_text, password)
        
        with pytest.raises(Exception):
            decrypt(encrypted, wrong_password)

    def test_decrypt_csharp_encrypted_value(self):
        """Test that we can decrypt a value encrypted by the C# implementation."""
        plain_text = "This is a secret"
        password = "password"
        
        # This value was encrypted by the C# LabKeyEncrypter
        encrypted = "HFtK6d+wgtbXMuaIVNTE1kpvb/M4+sOBbRnlq8RomRrWwVECOi4sTamwL19nXXpENvu8UTKO2owy2jf6916lJA=="
        
        decrypted = decrypt(encrypted, password)
        
        assert decrypted == plain_text

    def test_encrypt_empty_string(self):
        """Test encrypting an empty string."""
        plain_text = ""
        password = "password"
        
        encrypted = encrypt(plain_text, password)
        decrypted = decrypt(encrypted, password)
        
        assert decrypted == plain_text

    def test_encrypt_unicode_text(self):
        """Test encrypting unicode text."""
        plain_text = "Hello 世界 🌍"
        password = "password"
        
        encrypted = encrypt(plain_text, password)
        decrypted = decrypt(encrypted, password)
        
        assert decrypted == plain_text

    def test_encrypt_with_unicode_password(self):
        """Test encrypting with a unicode password."""
        plain_text = "This is a secret"
        password = "密码🔐"
        
        encrypted = encrypt(plain_text, password)
        decrypted = decrypt(encrypted, password)
        
        assert decrypted == plain_text
