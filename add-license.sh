
for i in $(find -iname "*.cs" -exec grep -L Copyright {} \;)
do
	echo $i
	cat license.cs $i >$i.new
	mv $i.new $i
done
