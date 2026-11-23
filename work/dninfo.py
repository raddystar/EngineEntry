import dnfile, sys, os

def info(path):
    dn = dnfile.dnPE(path)
    if not dn or not dn.net:
        print(f"{os.path.basename(path)}: not a .NET assembly")
        return
    print(f"=== {os.path.basename(path)} ===")
    n = dn.net
    try:
        print(f"  CLR version: {n.struct.VERSION.major}.{n.struct.VERSION.minor}")
    except Exception:
        pass
    md = n.metadata
    if md:
        print(f"  Metadata root: {md.rva:#x}")
        for sname, sobj in md.streams.items():
            print(f"  stream: {sname} type={type(sobj).__name__}")
    # Tables
    try:
        tables = n.mdtables
        print("  tables:", [t for t in dir(tables) if not t.startswith('_')])
    except Exception as e:
        print("  mdtables error:", e)

    # TypeDef
    try:
        td = tables.TypeDef
        print(f"  TypeDef count: {len(td.rows)}")
        for row in td.rows:
            ns = row.Namespace
            print(f"    {ns}.{row.Name}")
    except Exception as e:
        print("  TypeDef error:", e)

    # Module
    try:
        mod = tables.Module
        for row in mod.rows:
            print(f"  Module: {row.Name}")
    except Exception as e:
        print("  Module error:", e)

    # Assembly
    try:
        asm = tables.Assembly
        for row in asm.rows:
            print(f"  Assembly: {row.Name} v{row.MajorVersion}.{row.MinorVersion}.{row.BuildNumber}.{row.RevisionNumber}")
    except Exception as e:
        pass

if __name__ == '__main__':
    for p in sys.argv[1:]:
        info(p)