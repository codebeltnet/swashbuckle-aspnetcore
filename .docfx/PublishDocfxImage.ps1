$version = minver -i -t v -v w
docker tag classlibrary1-docfx:$version yourbranding/classlibrary1:$version
docker push yourbranding/classlibrary1:$version
