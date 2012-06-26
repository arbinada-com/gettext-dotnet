#!bin/sh
version=1.1
common_build_options=/target:Build /verbosity:quiet
product_name=GettextNet
release_dir=GettextNet
xbuild "./GNU.Gettext/GNU.Gettext/GNU.Gettext.sln" $common_build_options /property:configuration=Release 
xbuild "./GNU.Gettext/GNU.Gettext/GNU.Gettext.sln" $common_build_options /property:configuration=Debug
rm -f -r ./$release_dir
mkdir $release_dir
mkdir ./$release_dir/Bin
mkdir ./$release_dir/Win32
cp -r ./Bin/* ./$release_dir/Bin/
cp -r ./Win32/* ./$release_dir/Win32/

svn_repo_url=https://genielamp.svn.sourceforge.net/svnroot/gettextnet
revision=$(svn info -rHEAD $svn_repo_url | grep Revision: | cut -c11-)
echo Last revision is: $revision
svn checkout -q $svn_repo_url ./$release_dir/

rm $product_name*.zip
zip -9 -r -q $product_name".v"$version"r"$revision.zip ./$release_dir
rm -f -r ./$release_dir
