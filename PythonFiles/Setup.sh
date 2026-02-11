#!/bin/bash

echo "============================"
echo "Setting up Python environment"
echo "============================"

# Get the directory of this script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Check Python
if ! command -v python3 &> /dev/null; then
    echo "[ERROR] Python not found. Please install Python and ensure it's in your PATH."
    read -p "Press any key to continue..."
    exit 1
fi

# Create venv if it doesn't exist
if [ ! -d "venv" ]; then
    echo "Creating virtual environment..."
    python3 -m venv venv
else
    echo "Virtual environment already exists."
fi

# Activate venv
source venv/bin/activate
if [ $? -ne 0 ]; then
    echo "[ERROR] Failed to activate venv."
    read -p "Press any key to continue..."
    exit 1
fi

# Upgrade pip
python -m pip install --upgrade pip

# Install requirements
if [ -f "$SCRIPT_DIR/requirements.txt" ]; then
    echo "Installing remaining dependencies..."
    python -m pip install -r "$SCRIPT_DIR/requirements.txt"
else
    echo "[WARNING] No requirements.txt found"
fi

echo ""
echo "Setup complete!"
read -p "Press any key to continue..."