"""
ConfigureLabKeys - One-time lab configuration for Python labs.

Mimics the .NET ConfigureLabKeys behavior:
- Locates the keys directory
- Randomly selects an encrypted appsettings file
- Decrypts it using AES-256-GCM
- Converts JSON to .env format
- Copies .env to labs/python/
"""

import os
import sys
import json
import base64
import random
import hashlib
from pathlib import Path
from typing import Any

from cryptography.hazmat.primitives.ciphers.aead import AESGCM
from cryptography.exceptions import InvalidTag


# =============================================================================
# AES-256-GCM Decryption (from LabKeyEncrypter)
# =============================================================================

SALT_SIZE = 16      # 128 bit
KEY_SIZE = 32       # 256 bit
NONCE_SIZE = 12     # 96 bit (recommended for AES-GCM)
TAG_SIZE = 16       # 128 bit (AES-GCM authentication tag)
ITERATIONS = 600000 # OWASP recommended minimum for PBKDF2-SHA256


def _derive_key(password: str, salt: bytes) -> bytes:
    """Derive an encryption key from password using PBKDF2-SHA256."""
    return hashlib.pbkdf2_hmac('sha256', password.encode('utf-8'), salt, ITERATIONS, KEY_SIZE)


def _clear_bytes(data: bytearray) -> None:
    """Securely clear sensitive data from memory."""
    for i in range(len(data)):
        data[i] = 0


def decrypt_value(cipher_text: str, password: str) -> str:
    """
    Decrypt cipher text that was encrypted with AES-256-GCM.

    Args:
        cipher_text: Base64-encoded string containing salt + nonce + tag + ciphertext.
        password: The password used for encryption.

    Returns:
        The decrypted plain text.

    Raises:
        cryptography.exceptions.InvalidTag: If authentication fails (wrong password).
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


def decrypt_json_file(file_path: Path, password: str) -> dict[str, Any]:
    """
    Decrypt all values in an encrypted JSON file.

    Returns:
        Dictionary with decrypted values.
    """
    json_string = file_path.read_text(encoding='utf-8')
    encrypted_object = json.loads(json_string)

    decrypted_object = {}
    for key, value in encrypted_object.items():
        decrypted_text = decrypt_value(value, password)
        # Try to parse as JSON (for nested objects), otherwise use as string
        try:
            decrypted_object[key] = json.loads(decrypted_text)
        except json.JSONDecodeError:
            decrypted_object[key] = decrypted_text

    return decrypted_object


# =============================================================================
# JSON to .env Conversion
# =============================================================================

def flatten_json_to_env(data: dict[str, Any], prefix: str = "") -> list[str]:
    """
    Flatten a JSON object to .env format lines.

    - Flat string values: KEY=value
    - Nested objects: PARENTKEY__CHILDKEY=value (double underscore delimiter)
    """
    lines = []

    for key, value in data.items():
        # Build the full key name
        if prefix:
            full_key = f"{prefix}__{key}".upper()
        else:
            full_key = key.upper()

        if isinstance(value, dict):
            # Recursively flatten nested objects
            lines.extend(flatten_json_to_env(value, full_key))
        else:
            # Convert value to string
            str_value = str(value) if value is not None else ""
            # Quote values that contain special characters
            if any(c in str_value for c in [' ', '"', "'", '=', '#']):
                str_value = f'"{str_value}"'
            lines.append(f"{full_key}={str_value}")

    return lines


# =============================================================================
# Directory and File Discovery
# =============================================================================

def find_keys_directory(start_path: Path, verbose: bool = False) -> Path:
    """
    Find the keys directory by walking up the directory tree.

    Accepts either:
    - A directory named 'keys'
    - A directory that contains a 'keys' subdirectory with encrypted files
    """
    current_dir = start_path.resolve()

    while current_dir != current_dir.parent:
        # Case 1: current directory is 'keys'
        if current_dir.name.lower() == 'keys':
            return current_dir

        # Case 2: current directory contains a 'keys' subdirectory
        candidate = current_dir / 'keys'
        if candidate.is_dir() and _contains_encrypted_settings(candidate):
            return candidate

        if verbose:
            print(f"Searching: {current_dir}")

        current_dir = current_dir.parent

    raise FileNotFoundError(
        "Unable to locate a 'keys' directory containing encrypted appsettings files."
    )


def _contains_encrypted_settings(keys_path: Path) -> bool:
    """Check if directory contains encrypted settings files."""
    return any(keys_path.glob("*.appsettings.Local_encrypted.json"))


def randomly_select_encrypted_file(keys_path: Path) -> Path:
    """Randomly select one of the encrypted settings files."""
    pattern = "*.appsettings.Local_encrypted.json"
    files = list(keys_path.glob(pattern))

    if not files:
        raise FileNotFoundError(
            f"No encrypted appsettings files found in {keys_path}"
        )

    return random.choice(files)


def find_labs_directory(keys_dir: Path) -> Path | None:
    """Find the labs directory by walking up from keys."""
    current = keys_dir
    while current != current.parent:
        if current.name.lower() == 'labs':
            return current
        current = current.parent
    return None


# =============================================================================
# Main Configuration Class
# =============================================================================

class ConfigureLabKeys:
    """
    One-time lab configuration:
    - Locate the keys directory
    - If .env already exists in labs/python, do nothing (unless overwrite_existing)
    - Randomly pick an encrypted appsettings file, decrypt it
    - Convert to .env format and save to labs/python/.env
    """

    def __init__(self, password: str, verbose: bool = False):
        self.password = password
        self.verbose = verbose

    def randomize_decrypt_distribute(
        self,
        start_path: str = ".",
        overwrite_existing: bool = False
    ) -> None:
        """
        Main entry point: find keys, decrypt, convert to .env, distribute.
        """
        keys_path = find_keys_directory(Path(start_path), self.verbose)

        # Find labs directory to determine .env output path
        labs_dir = find_labs_directory(keys_path)
        if labs_dir is None:
            raise FileNotFoundError(
                "Could not find 'labs' directory above keys directory."
            )

        python_labs_dir = labs_dir / "python"
        env_file_path = python_labs_dir / ".env"

        # Check if .env already exists
        if env_file_path.exists() and not overwrite_existing:
            if self.verbose:
                print("Skipping lab configuration: .env already present.")
                print(f"  {env_file_path}")
            return

        if env_file_path.exists() and overwrite_existing and self.verbose:
            print("Overwriting lab configuration: .env already present.")
            print(f"  {env_file_path}")

        print("One-time lab configuration started.")

        # Randomly select and decrypt
        encrypted_file = randomly_select_encrypted_file(keys_path)
        if self.verbose:
            print(f"Selected encrypted settings: {encrypted_file.name}")

        try:
            decrypted_data = decrypt_json_file(encrypted_file, self.password)
        except InvalidTag:
            print(f"Error decrypting {encrypted_file.name}: Authentication failed.")
            print(f"Double-check that password '{self.password}' is correct.")
            sys.exit(1)

        # Convert to .env format
        env_lines = flatten_json_to_env(decrypted_data)
        env_content = "\n".join(env_lines) + "\n"

        # Write .env file
        python_labs_dir.mkdir(parents=True, exist_ok=True)
        env_file_path.write_text(env_content, encoding='utf-8')

        if self.verbose:
            print(f"Created .env file at {env_file_path}")
            print(f"Environment variables written: {len(env_lines)}")

        print("One-time lab configuration completed.")


# =============================================================================
# CLI Entry Point (optional - can be run standalone)
# =============================================================================

def main() -> None:
    """CLI entry point for standalone usage."""
    import argparse

    parser = argparse.ArgumentParser(
        description="Configure Python labs by decrypting settings and creating .env"
    )
    parser.add_argument(
        "--password", "-p",
        required=True,
        help="Decryption password (ask your lab instructor)"
    )
    parser.add_argument(
        "--force", "--overwrite",
        action="store_true",
        help="Overwrite existing .env file"
    )
    parser.add_argument(
        "--verbose", "-v",
        action="store_true",
        help="Enable verbose output"
    )

    args = parser.parse_args()

    configger = ConfigureLabKeys(args.password, args.verbose)
    configger.randomize_decrypt_distribute(overwrite_existing=args.force)


if __name__ == "__main__":
    main()
