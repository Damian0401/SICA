import { useState } from 'react'
import './App.css'
import { Box, Button, Card, Flex, Text, TextArea } from '@radix-ui/themes'
import { queryFiles, uploadFile } from './service/fileApi';

function App() {
  const [file, setFile] = useState<File | null>(null);
  const [fileResult, setFileResult] = useState(null)
  const [query, setQuery] = useState<string>("")

  const handleUpload = async () => {
    if (!file) return;
    try {
      const result = await uploadFile(file);
      console.log('Upload result:', result);
    } catch (err) {
      console.error(err);
    }
  };

  const handleQuery = async () => {
    try {
      const result = await queryFiles(query);
      console.log('Query result:', result);
      setFileResult(result.files)
    } catch (err) {
      console.error(err);
    }
  };
  return (
    <Box className='main-wrapper'>
    <Flex direction="column" className='cards-wrapper' gap="5">
      <Card>
        <Flex direction="column" gap="2">
        <Text as="div" size="2" weight="bold">
          Upload CV
        </Text>
        <input type='file' onChange={(e) => setFile(e.target.files?.[0] || null)}/>
        <Button color="gray" variant="solid" highContrast onClick={handleUpload}>Upload</Button>
        </Flex>
      </Card>
      <Card>
        <Flex direction="column" gap="2">
        <Text as="div" size="2" weight="bold">
          Search
        </Text>
        <TextArea onChange={(e) => setQuery(e.target.value)}/>
        <Button color="gray" variant="solid" highContrast onClick={handleQuery}>Search</Button>
        {fileResult && fileResult.map((file: any) => (
  <Card key={file.id} style={{ marginTop: '1rem' }}>
    <Text as="div" size="2"><strong>Name:</strong> {file.fileName}</Text>
    <Text as="div" size="2"><strong>Score:</strong> {file.score}</Text>
  </Card>
))}
        </Flex>
      </Card>
    </Flex>
    </Box>
  )
}

export default App
