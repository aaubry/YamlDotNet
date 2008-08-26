#!/bin/sh
find -not -iwholename "./YamlDotNet.UnitTests*" -iname "*.cs" -exec astyle {} \; -exec sed -i -n '1h;1!H;${;g;s/\(where[^:]*:\)[\r\n\t]*/\1 /g;s/\(\t[^\n]*[)] where\)/\t\1/g;s/:\s*\(\(base\|this\)\)/\t\t: \1/g;s/[?] [?]/??/g;p;}' {} \;
find -iname "*orig" -exec rm {} \;

