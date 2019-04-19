$tag = git log --pretty=format:%h -n 1
docker tag docker-bottomly bottomlycontainerreg.azurecr.io/docker-bottomly:$tag
docker tag docker-bottomly bottomlycontainerreg.azurecr.io/docker-bottomly:latest
docker images