
import React, { useRef, useState } from 'react';
import { Button } from '@/components/ui/button';
import { Upload } from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';

interface CVUploadProps {
  onUpload: (file: File) => Promise<boolean>;
}

const CVUpload: React.FC<CVUploadProps> = ({ onUpload }) => {
  const [uploading, setUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const { toast } = useToast();

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    
    if (!files || files.length === 0) {
      return;
    }

    const file = files[0];
    if (!file.name.endsWith('.pdf') && !file.name.endsWith('.docx') && !file.name.endsWith('.txt')) {
      toast({
        title: "Invalid file format",
        description: "Please upload a PDF, DOCX, or TXT file",
        variant: "destructive"
      });
      return;
    }

    setUploading(true);
    try {
      await onUpload(file);
      // Clear the file input for future uploads
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    } finally {
      setUploading(false);
    }
  };

  const handleButtonClick = () => {
    fileInputRef.current?.click();
  };

  return (
    <div className="flex flex-col items-center justify-center border-2 border-dashed border-gray-200 rounded-lg p-8 bg-gray-50">
      <Upload className="h-8 w-8 text-gray-400 mb-2" />
      <h3 className="text-lg font-medium text-gray-700 mb-2">Upload a CV</h3>
      <p className="text-sm text-gray-500 text-center mb-4">
        PDF, DOCX or TXT (Max 5MB)
      </p>
      
      <input
        type="file"
        ref={fileInputRef}
        onChange={handleFileChange}
        accept=".pdf,.docx,.txt"
        className="hidden"
      />
      
      <Button
        onClick={handleButtonClick}
        className="apple-button-primary"
        disabled={uploading}
      >
        {uploading ? "Uploading..." : "Browse Files"}
      </Button>
    </div>
  );
};

export default CVUpload;
