$version = minver -i -t v -v w
docker tag swashbuckle-aspnetcore-docfx:$version jcr.codebelt.net/geekle/swashbuckle-aspnetcore-docfx:$version
docker push jcr.codebelt.net/geekle/swashbuckle-aspnetcore-docfx:$version
