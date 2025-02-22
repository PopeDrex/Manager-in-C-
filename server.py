from flask import Flask, request, jsonify
import json
import os
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.kdf.scrypt import Scrypt
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives import padding
import base64
import os

app = Flask(__name__)

NOTES_FILE = 'notes.json'
ENCRYPTION_KEY = b'078c945060c98b663c1e7ac0a8ddec8692bab3458d15b6a4e6490c2c00afd07b'  # 32 bytes key

def encrypt(plain_text):
    salt = os.urandom(16)
    kdf = Scrypt(salt=salt, length=32, n=2**14, r=8, p=1, backend=default_backend())
    key = kdf.derive(ENCRYPTION_KEY)
    iv = os.urandom(16)
    cipher = Cipher(algorithms.AES(key), modes.CBC(iv), backend=default_backend())
    encryptor = cipher.encryptor()
    padder = padding.PKCS7(algorithms.AES.block_size).padder()
    padded_data = padder.update(plain_text.encode()) + padder.finalize()
    encrypted = encryptor.update(padded_data) + encryptor.finalize()
    return base64.b64encode(salt + iv + encrypted).decode('utf-8')

def decrypt(encrypted_text):
    encrypted_data = base64.b64decode(encrypted_text)
    salt = encrypted_data[:16]
    iv = encrypted_data[16:32]
    encrypted = encrypted_data[32:]
    kdf = Scrypt(salt=salt, length=32, n=2**14, r=8, p=1, backend=default_backend())
    key = kdf.derive(ENCRYPTION_KEY)
    cipher = Cipher(algorithms.AES(key), modes.CBC(iv), backend=default_backend())
    decryptor = cipher.decryptor()
    decrypted_padded = decryptor.update(encrypted) + decryptor.finalize()
    unpadder = padding.PKCS7(algorithms.AES.block_size).unpadder()
    decrypted = unpadder.update(decrypted_padded) + unpadder.finalize()
    return decrypted.decode('utf-8')

def load_notes():
    if os.path.exists(NOTES_FILE):
        with open(NOTES_FILE, 'r') as file:
            encrypted_notes = json.load(file)
            return [json.loads(decrypt(note)) for note in encrypted_notes]
    return []

def save_notes(notes):
    encrypted_notes = [encrypt(json.dumps(note)) for note in notes]
    with open(NOTES_FILE, 'w') as file:
        json.dump(encrypted_notes, file)

@app.route('/', methods=['POST'])
def save_notes_endpoint():
    try:
        notes = request.json
        if not isinstance(notes, list):
            return jsonify({"error": "Invalid data format. Expected a list of notes."}), 400
        save_notes(notes)
        return jsonify({"message": "Notes saved successfully."}), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500

@app.route('/', methods=['GET'])
def get_notes_endpoint():
    try:
        notes = load_notes()
        return jsonify(notes), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)
