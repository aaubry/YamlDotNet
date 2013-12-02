#!/bin/bash

main () {
  for f in $(find -iname "*.cs" | grep -v "./_ReSharper" | grep -v "/obj/")
  do
    license $f
    crlf_endings $f
  done
}

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
  perl -pe 's/\r?\n/\r\n/' $1 >$1.new
  mv $f.new $f
}

main
