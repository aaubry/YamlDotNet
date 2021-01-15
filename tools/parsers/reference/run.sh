
if [ "$1" == "--html" ]; then
    /app/dist/build/yaml2yeast/yaml2yeast <&0 | /app/yeast2html
else
    echo "Use --html for HTML output"
    /app/dist/build/yaml2yeast/yaml2yeast <&0 | grep --color -E '^!.*|$'
fi
