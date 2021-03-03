# SIL.Windows.Forms Test App

## Running on Linux

Before running this test app on Linux, source the `environ` file.

Alternatively you can manually set three environment variables:

- `LD_PRELOAD` to the path to `libgeckofix.so` (`.../output/Debug/net461/Firefox/libgeckofix.so`).
- `LD_LIBRARY_PATH` to `.../output/Debug/net461/Firefox:/usr/lib`
- `XULRUNNER` to `.../output/Debug/net461/Firefox`
