import sys
import json
import re
import argparse

parser = argparse.ArgumentParser(description="Converts Binance's REST output to JSON adhered to their own format.")
parser.add_argument("-event")
parser.add_argument("-symbol")
parser.add_argument("-interval")
args = parser.parse_args()

def wrap(rows, base_k = {'s': args.symbol, 'i': args.interval}, wrapper = {'e': args.event, 's': args.symbol}):
    for row in rows:
        result_row = base_k
        for (column, type, cell) in zip(COLUMNS, TYPES, row):
            result_row[column] = type(cell)
        wrapper['k'] = result_row
        yield json.dumps(wrapper)

# columns are defined positionally
COLUMNS = [
    "t",    # Open time
    "o",    # Open
    "h",    # High
    "l",    # Low
    "c",    # Close
    "v",    # Volume
    "T",    # Close time
    "q",    # Quote asset volume
    "n",    # Number of trades
    "V",    # Taker buy base asset volume
    "Q",    # Taker buy quote asset volume
    "B"     # Ignore
]
TYPES = [
    int,
    float,
    float,
    float,
    float,
    float,
    int,
    float,
    int,
    float,
    float,
    float
]
# matches one candlestick
pattern = re.compile(r"\[?\[(.*?)\],?\]?")
# read stdin line per line
buffer = []
for line in sys.stdin:
    buffer.append(line)
    if line.endswith("\n"):
        buffer = "".join(buffer).replace('"', '')
        # find all candlesticks
        matches = re.findall(pattern, buffer)
        # massage data a bit
        rows = (m.split(",") for m in matches)
        # zip everything together
        sys.stdout.write(f"[{','.join(wrap(rows))}]\n")
        # clean-up
        sys.stdout.flush()
        buffer = []