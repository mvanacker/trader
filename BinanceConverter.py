import sys
import json
import argparse
import datetime

parser = argparse.ArgumentParser(description="Converts Binance's JSON to SQL.")
parser.add_argument("-prefix")
args = parser.parse_args()

ROSETTA = {
    "t": "startTimestamp",
    "T": "timestamp",
    "s": "symbol",
    "i": "interval",
    "f": "firstTradeID",
    "L": "lastTradeID",
    "h": "high",
    "l": "low",
    "o": "open",
    "c": "close",
    "v": "baseVolume",
    "q": "volume",
    "n": "trades",
    "V": "takerBuyBaseVolume",
    "Q": "takerBuyVolume",
    "x": "closed",
    "B": "ignore"
}
DATA_TYPES = {
    "t": "datetime",
    "T": "datetime",
    "s": "varchar(255)",
    "i": "varchar(255)",
    "f": "bigint",
    "L": "bigint",
    "h": "float",
    "l": "float",
    "o": "float",
    "c": "float",
    "v": "float",
    "q": "float",
    "n": "bigint",
    "V": "float",
    "Q": "float",
    "x": "char(5)",
    "B": "float"
}

def columns(datum):
    return ",".join([f"[{ROSETTA[key]}] {DATA_TYPES[key]} null" for key in datum])

def create_table(table, datum):
    return f"if not exists (select [name] from sys.tables where name = '{table}') create table [{table}] ({columns(datum)});"

def insert_into(table, datum, format = "%Y-%m-%d %H:%M:%S.%f"):
    columns, values = [], []
    for letter, value in datum.items():
        columns.append(ROSETTA[letter])
        if DATA_TYPES[letter] is "datetime":
            value /= 1000.0
            value = datetime.datetime.fromtimestamp(value).strftime(format)[:-3]
        values.append(convert(value))
    return f"insert into [{table}] ([{'],['.join(columns)}]) values ({','.join(values)});"

def convert(value):
    if value is None:
        return "null"
    elif type(value) is int or type(value) is float:
        return str(value)
    else:
        #value = str(value).replace("\n", " ")
        return f"'{value}'"
    
creations = set()
kline_data = {}
buffer = []
for line in sys.stdin:
    buffer.append(line)
    if line.endswith("\n"):
        # pseudo-stringbuilder for our sql query
        sql = []
        # parse json
        buffer = "".join(buffer)
        json_objects = json.loads(buffer)
        if type(json_objects) is not list:
            json_objects = [json_objects]
        for json_object in json_objects:
            # unwrap raw data from a combined stream
            if "stream" in json_object:
                json_object = json_object['data']
            # distill the raw data
            raw_data = json_object['k']
            label = json_object['e'] # event type as the default label
            symbol = json_object['s']
            # for klines, append their interval to the label
            if label == "kline":
                label = f"{label}_{raw_data['i']}"
            # prepend the argued prefix to obtain the table's name
            table = f"{args.prefix}_{label}"
            # table creation sql
            if table not in creations:
                sql.append(create_table(table, raw_data))
                creations.add(table)
            # insertion sql
            # for klines, insert only if this kline's start time is higher than the previous
            if json_object['e'] != "kline" or (label in kline_data and symbol in kline_data[label] and raw_data['t'] > kline_data[label][symbol]['t']):
                sql.append(insert_into(table, kline_data[label][symbol]))
            # store raw kline data for future use (will be overwritten many times for live data)
            if json_object['e'] == "kline":
                if label not in kline_data:
                    kline_data[label] = {}
                kline_data[label][symbol] = raw_data
        # output resulting sql string
        sys.stdout.write(f"{''.join(sql)}\n")
        # clean-up
        sys.stdout.flush()
        buffer = []
