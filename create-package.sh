#!bin/sh
sh ./build-all.sh

svn_repo_url=https://svn.code.sf.net/p/gettextnet/code
version=1.1
product_name=GettextNet
release_dir=GettextNet

rm -f -r ./$release_dir
mkdir $release_dir
mkdir ./$release_dir/Bin
mkdir ./$release_dir/Win32
cp -r ./Bin/* ./$release_dir/Bin/
cp -r ./Win32/* ./$release_dir/Win32/
svn checkout -q $svn_repo_url ./$release_dir/

revision=$(svn info -rHEAD $svn_repo_url | grep Revision: | cut -c11-)
echo Last revision is: $revision

rm $product_name*.zip
zip -9 -r -q $product_name".v"$version"r"$revision.zip ./$release_dir -x *.svn*
rm -f -r ./$release_dir
