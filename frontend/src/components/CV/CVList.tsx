
import React from 'react';
import { useCVs } from '@/hooks/useCVs';
import CVCard from './CVCard';
import CVUpload from './CVUpload';
import { Skeleton } from '@/components/ui/skeleton';

const CVList: React.FC = () => {
  const { cvs, loading, uploadCV, deleteCV } = useCVs();
  
  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-medium text-gray-900 mb-1">CV Repository</h2>
        <p className="text-sm text-gray-500 mb-4">
          Upload and manage your CV collection
        </p>
      </div>
      
      <CVUpload onUpload={uploadCV} />
      
      <div>
        <h3 className="text-base font-medium text-gray-700 mb-3">Uploaded CVs</h3>
        
        {loading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {[1, 2, 3].map((i) => (
              <div key={i} className="border rounded-lg p-4">
                <Skeleton className="h-4 w-3/4 mb-3" />
                <Skeleton className="h-3 w-1/2 mb-2" />
                <Skeleton className="h-3 w-1/3" />
              </div>
            ))}
          </div>
        ) : cvs.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg border border-gray-100">
            <p className="text-gray-500">No CVs uploaded yet</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {cvs.map((cv) => (
              <CVCard key={cv.id} cv={cv} onDelete={deleteCV} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default CVList;
