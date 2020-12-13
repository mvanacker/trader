import sys
import json
import argparse

parser = argparse.ArgumentParser(description="Converts BitMEX' JSON to SQL.")
parser.add_argument("-prefix")
args = parser.parse_args()

DATA_TYPES = {
    "symbol": "varchar(255)",
    "long": "bigint",
    "float": "float",
    "boolean": "char(5)",
    "timestamp": "datetime",
    "guid": "uniqueidentifier"
}
primary_keys = {}
partials = set()
partials.add("tradeHistory")
partials.add("trade")
partials.add("tradeBin5m")
partials.add("volumeBin1mil")
partials.add("volumeBin10mil")
partials.add("volumeBin100mil")
partials.add("volumeBin1bil")
always_drop = set()
always_drop.add(f"{args.prefix}_order")
always_drop.add(f"{args.prefix}_margin")
always_drop.add(f"{args.prefix}_position")
always_drop.add(f"{args.prefix}_orderBookL2")

def on_partial(dictionary):
    table = dictionary['table']
    keys = dictionary['keys']
    create = ""
    if not table in primary_keys:
        primary_keys[table] = keys
        sql = []
        if table in always_drop:
            sql.append(f"drop table if exists [{table}];")
        else:
            sql.append(f"if not exists (select [name] from sys.tables where [name] = '{table}')")
        sql.append(f"create table [{table}] (")
        columns = [f"[{key}] {DATA_TYPES[value]}" for key, value in dictionary['types'].items()]
        sql.append(",".join(columns))
        if len(keys):
            sql.append(f" constraint [pk_{table}] primary key ([{'],['.join(keys)}])")
        create = f"{''.join(sql)});"
    return f"{create}{on_insert(dictionary)}"

def on_insert(dictionary):
    return general_query(dictionary, insert_into)

def on_update(dictionary):
    return general_query(dictionary, update)

def on_delete(dictionary):
    return general_query(dictionary, delete_from)

def general_query(dictionary, command):
    return ";".join([command(dictionary['table'], datum) for datum in dictionary['data']])

def insert_into(table, datum):
    columns, values = [], []
    for column, value in datum.items():
        columns.append(column)
        values.append(convert(value))
    return f"insert into [{table}] ([{'],['.join(columns)}]) values ({','.join(values)})"

def update(table, datum):
    assignments = [f"[{column}]={convert(value)}" for column, value in datum.items() if column not in primary_keys[table]]
    return f"update [{table}] set {','.join(assignments)} {where(table, datum)}"

def delete_from(table, datum):
    return f"delete from [{table}] {where(table, datum)}"

def where(table, datum):
    conditions = [f"[{key}]={convert(datum[key])}" for key in primary_keys[table]]
    return f"where {' and '.join(conditions)}"

def convert(value):
    if value is None:
        return "null"
    elif type(value) is int or type(value) is float:
        return str(value)
    else:
        value = str(value).replace("\n", " ")
        return f"'{value}'"

SWITCH = { "partial": on_partial, "insert": on_insert, "update": on_update, "delete": on_delete }
buffer = []
for line in sys.stdin:
    buffer.append(line)
    if line.endswith("\n"):
        buffer = "".join(buffer)
        dictionary = json.loads(buffer)
        if "table" in dictionary:
            table = dictionary['table']
            action = dictionary['action']
            if action == "partial":
                partials.add(table)
            if table in partials:
                dictionary['table'] = f"{args.prefix}_{table}"
                sys.stdout.write(SWITCH[action](dictionary))
        sys.stdout.write("\n")
        sys.stdout.flush()
        buffer = []
