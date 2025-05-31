
import React from 'react';
import { CV } from '@/types';
import { Button } from '@/components/ui/button';
import { FileText, Trash2, Download } from 'lucide-react';
import { format, isValid } from 'date-fns';
import { fileService } from '@/services/fileService';

interface CVCardProps {
  cv: CV;
  onDelete: (id: string) => void;
}

const CVCard: React.FC<CVCardProps> = ({ cv, onDelete }) => {
  const handleDownload = () => {
    // Only attempt download for files that have an ID from the backend
    if (cv.id) {
      const downloadUrl = fileService.getFileDownloadUrl(cv.id);
      window.open(downloadUrl, '_blank');
    }
  };
  
  // Safely format the date
  const formatDate = (date: Date): string => {
    try {
      // Verify if the date is valid before formatting
      if (isValid(date)) {
        return format(date, 'MMM d, yyyy');
      }
      return 'Date unavailable';
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Date unavailable';
    }
  };

  return (
    <div className="apple-card p-4 flex flex-col">
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center">
          <FileText className="h-5 w-5 text-primary mr-2" />
          <h3 className="font-medium text-gray-900 truncate max-w-[150px] sm:max-w-[200px]">
            {cv.name}
          </h3>
        </div>
        <div className="flex">
          {!cv.content && cv.id && (
            <Button
              variant="ghost"
              size="icon"
              onClick={handleDownload}
              className="h-8 w-8 text-gray-400 hover:text-primary mr-1"
              aria-label={`Download ${cv.name}`}
            >
              <Download className="h-4 w-4" />
            </Button>
          )}
          <Button
            variant="ghost"
            size="icon"
            onClick={() => onDelete(cv.id)}
            className="h-8 w-8 text-gray-400 hover:text-destructive"
            aria-label={`Delete ${cv.name}`}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      </div>
      <div className="text-xs text-gray-500 mb-3">
        <p className="truncate">{cv.fileName}</p>
        <p className="mt-1">
          Uploaded on {formatDate(cv.uploadDate)}
        </p>
      </div>
    </div>
  );
};

export default CVCard;
