
license () {
  f=$1
  if ! grep -q Copyright $f ;
  then
    echo "Adding licence to $f"
    cat license.cs $f >$f.new
    mv $f.new $f
  fi
}

crlf_endings () {
  sed -i 's/\r\?$/\r/' $1
}

for f in $(find -iname "*.cs")
do
  license $f
  crlf_endings $f
done

