docker stop bottomly-test
docker build --tag bottomly-test -f test.dockerfile .
docker run --rm --name bottomly-test --env-file .env bottomly