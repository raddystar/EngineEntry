import cabarchive, os, sys

src = sys.argv[1]
outdir = sys.argv[2]
os.makedirs(outdir, exist_ok=True)

c = cabarchive.CabFile(src)
for f in c.files:
    name = f.name
    safe = name.replace('\\', '_')
    data = f.read()
    path = os.path.join(outdir, safe)
    with open(path, 'wb') as fh:
        fh.write(data)
    print(f"{name} -> {path} ({len(data)} bytes)")
print("done")