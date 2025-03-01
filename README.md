# fly-dist-sys
A series of distributed systems challenges brought to you by Fly.io.

## Prerequisites
```sh
docker pull ghcr.io/0xc0dec0ffeelab/maelstrom:latest
```

## Build
```sh
dotnet publish -r linux-x64 -c Release --self-contained true -p:PublishSingleFile=true -o publish
```
## Test

### Echo
> Windows
```sh
MSYS_NO_PATHCONV=1 docker run --rm \
    -v $HOME/fly-dist-sys/fly-dist-sys/publish:/maelstrom/mnt ghcr.io/0xc0dec0ffeelab/maelstrom:latest test
    -w echo \
    --bin /maelstrom/mnt/fly-dist-sys --node-count 1 --time-limit 10

```

* If you want to check logs at `$HOME/fly-dist-sys/fly-dist-sys/logs`
```sh
 MSYS_NO_PATHCONV=1 docker run --rm -v $HOME/fly-dist-sys/fly-dist-sys/publish:/maelstrom/mnt \
     -v $HOME/fly-dist-sys/fly-dist-sys/logs:/maelstrom/store \
     ghcr.io/0xc0dec0ffeelab/maelstrom:latest test -w echo --bin \
     /maelstrom/mnt/fly-dist-sys --node-count 1 --time-limit 10
```
