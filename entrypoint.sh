#!/bin/sh

ln -s /lib/libc.musl-x86_64.so.1 /lib/ld-linux-x86-64.so.2
exec dotnet Nop.Web.dll

set -e
service ssh start
exec gunicorn -w 4 -b 0.0.0.0:8000 app:app