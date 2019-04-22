docker build --tag docker-bottomly .
docker system prune -f
az acr login --name BottomlyContainerReg
$tag = git log --pretty=format:%h -n 1
docker tag docker-bottomly bottomlycontainerreg.azurecr.io/docker-bottomly:$tag
docker push bottomlycontainerreg.azurecr.io/docker-bottomly:$tag
docker images