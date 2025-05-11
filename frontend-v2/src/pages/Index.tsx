
import React, { useState } from 'react';
import MainLayout from '../components/Layout/MainLayout';
import TabNavigation from '../components/Tabs/TabNavigation';

const Index: React.FC = () => {
  const [activeTab, setActiveTab] = useState('cvs');
  
  return (
    <MainLayout>
      <TabNavigation 
        activeTab={activeTab} 
        onChangeTab={setActiveTab} 
      />
    </MainLayout>
  );
};

export default Index;
