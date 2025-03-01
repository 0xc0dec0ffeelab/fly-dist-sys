# fly-dist-sys
A series of distributed systems challenges brought to you by Fly.io.

## Prerequisites
```sh
docker pull ghcr.io/0xc0dec0ffeelab/maelstrom:latest
```

## Build
```sh
dotnet publish -r linux-x64 -c Release --self-contained true -p:PublishSingleFile=true -p:DefineConstants=<Challenge_Constant> -o publish
```

| Challenge_Constant                               | Name                                                |
|------------------------------------------------|-----------------------------------------------------|
| Challenge_1                                    | Echo                                               |
| Challenge_2                                    | Unique ID Generation                               |
| Challenge_3a                                   | Single Node Broadcast                             |
| Challenge_3b                                   | Multi Node Broadcast                              |
| Challenge_3c                                   | Fault Tolerant Broadcast                          |
| Challenge_3d                                   | Efficient Broadcast Part 1                        |
| Challenge_3e                                   | Efficient Broadcast Part 2                        |
| Challenge_4                                    | Grow Only Counter                                 |
| Challenge_5a                                   | Single Node Kafka Style Log                       |
| Challenge_5b                                   | Multi Node Kafka Style Log                        |
| Challenge_5c                                   | Efficient Kafka Style Log                         |
| Challenge_6a                                   | Single Node Totally Available Transactions        |
| Challenge_6b                                   | Totally Available Read Uncommitted Transactions   |
| Challenge_6c                                   | Totally Available Read Committed Transactions     |




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
