#!/bin/bash

echo "============================"
echo "Generating clean requirements.txt (removing CUDA suffixes)"
echo "============================"

# Use the virtual environment's Python
VENV_PY="$(dirname "$0")/venv/bin/python"

if [ ! -f "$VENV_PY" ]; then
    echo "Virtual environment not found!"
    echo "Please ensure 'venv' exists next to this script."
    read -p "Press any key to continue..."
    exit 1
fi

# Create temporary raw requirements file
echo "Freezing packages..."
"$VENV_PY" -m pip freeze > "$(dirname "$0")/requirements_raw.txt"

# Remove CUDA suffixes (e.g., +cu121, +cu126, etc.)
echo "Cleaning CUDA suffixes..."
sed 's/+cu[0-9]\+//g' "$(dirname "$0")/requirements_raw.txt" > "$(dirname "$0")/requirements.txt"

# Delete temp file
rm "$(dirname "$0")/requirements_raw.txt"

echo "============================"
echo "Done! Clean requirements.txt created."
echo "============================"
read -p "Press any key to continue..."