
import React from 'react';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import CVList from '../CV/CVList';
import JobDescriptionForm from '../JobDescription/JobDescriptionForm';

interface TabNavigationProps {
  activeTab?: string;
  onChangeTab?: (value: string) => void;
}

const TabNavigation: React.FC<TabNavigationProps> = ({ 
  activeTab = "cvs",
  onChangeTab = () => {},
}) => {
  return (
    <Tabs 
      defaultValue={activeTab} 
      onValueChange={onChangeTab}
      className="w-full"
    >
      <TabsList className="grid w-full grid-cols-2 mb-6">
        <TabsTrigger value="cvs" className="text-sm sm:text-base">CVs</TabsTrigger>
        <TabsTrigger value="job" className="text-sm sm:text-base">Job Description</TabsTrigger>
      </TabsList>
      
      <TabsContent value="cvs" className="tab-panel">
        <CVList />
      </TabsContent>
      
      <TabsContent value="job" className="tab-panel">
        <JobDescriptionForm />
      </TabsContent>
    </Tabs>
  );
};

export default TabNavigation;
