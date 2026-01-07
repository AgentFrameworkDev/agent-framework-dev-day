# Python Labs

Before entering a specific lab folder, activate the Python `venv` virtual environment (one line), install common dependencies (one line), then `cd` into a lab folder and start coding!

## Windows (Command Prompt)

```console
# from .../labs/python
venv\Scripts\activate
# command prompt is now (.venv)
pip install -r requirements.txt
```console

## Windows (PowerShell)

```PowerShell
# from .../labs/python
env\Scripts\Activate.ps1
# command prompt is now (.venv)
pip install -r requirements.txt
```

If you get an error about execution policies, consider this:

```PowerShell
CopySet-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

## Mac, Linux

```console
# from .../labs/python
source .venv/bin/activate
# command prompt is now (.venv)
pip install -r requirements.txt
# this changes prompt to (.venv)
```

## Start Labs

```console
cd lab0
python main.py
```

Onward!

## If you want to be tidy at the end

```console
# from any folder, as long as prompt is still (.venv)
deactivate
```
