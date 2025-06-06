#!/bin/bash

/bin/ollama serve &
pid=$!

echo "Waiting for Ollama server to be active..."
while [ "$(ollama list | grep 'NAME')" == "" ]; do
  sleep 1
done

echo "Pull models"
ollama pull nomic-embed-text:v1.5
ollama pull gemma3:1b

wait $pid
