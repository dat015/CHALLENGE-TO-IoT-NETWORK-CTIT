import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Grid,
  Typography,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  IconButton,
  Chip,
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon } from '@mui/icons-material';
import { Plant, Sensor } from '../types/plant';

const PlantManagement: React.FC = () => {
  const [plants, setPlants] = useState<Plant[]>([]);
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedPlant, setSelectedPlant] = useState<Plant | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    type: '',
    plantingDate: '',
    status: '',
  });

  useEffect(() => {
    // TODO: Fetch plants from API
    // For now using mock data
    const mockPlants: Plant[] = [
      {
        id: 1,
        name: 'Tomato Plant',
        type: 'Vegetable',
        plantingDate: '2024-03-15',
        status: 'Growing',
        sensors: [
          {
            id: 1,
            type: 'Temperature',
            value: 25,
            unit: '°C',
            lastUpdated: new Date().toISOString(),
          },
          {
            id: 2,
            type: 'Humidity',
            value: 60,
            unit: '%',
            lastUpdated: new Date().toISOString(),
          },
        ],
      },
    ];
    setPlants(mockPlants);
  }, []);

  const handleOpenDialog = (plant?: Plant) => {
    if (plant) {
      setSelectedPlant(plant);
      setFormData({
        name: plant.name,
        type: plant.type,
        plantingDate: plant.plantingDate,
        status: plant.status,
      });
    } else {
      setSelectedPlant(null);
      setFormData({
        name: '',
        type: '',
        plantingDate: '',
        status: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setSelectedPlant(null);
  };

  const handleSubmit = () => {
    if (selectedPlant) {
      // Update existing plant
      setPlants(plants.map(plant =>
        plant.id === selectedPlant.id
          ? { ...plant, ...formData }
          : plant
      ));
    } else {
      // Add new plant
      const newPlant: Plant = {
        id: plants.length + 1,
        ...formData,
        sensors: [],
      };
      setPlants([...plants, newPlant]);
    }
    handleCloseDialog();
  };

  const handleDeletePlant = (plantId: number) => {
    setPlants(plants.filter(plant => plant.id !== plantId));
  };

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h4">Plant Management</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
        >
          Add New Plant
        </Button>
      </Box>

      <Grid container spacing={3}>
        {plants.map((plant) => (
          <Grid item xs={12} sm={6} md={4} key={plant.id}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                  <Typography variant="h6">{plant.name}</Typography>
                  <Box>
                    <IconButton onClick={() => handleOpenDialog(plant)}>
                      <EditIcon />
                    </IconButton>
                    <IconButton onClick={() => handleDeletePlant(plant.id)}>
                      <DeleteIcon />
                    </IconButton>
                  </Box>
                </Box>
                <Typography color="textSecondary">Type: {plant.type}</Typography>
                <Typography color="textSecondary">
                  Planting Date: {plant.plantingDate}
                </Typography>
                <Typography color="textSecondary">Status: {plant.status}</Typography>
                
                <Typography variant="subtitle1" sx={{ mt: 2, mb: 1 }}>
                  Sensors:
                </Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                  {plant.sensors.map((sensor) => (
                    <Chip
                      key={sensor.id}
                      label={`${sensor.type}: ${sensor.value}${sensor.unit}`}
                      color="primary"
                      variant="outlined"
                    />
                  ))}
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Dialog open={openDialog} onClose={handleCloseDialog}>
        <DialogTitle>
          {selectedPlant ? 'Edit Plant' : 'Add New Plant'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
            <TextField
              label="Name"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              fullWidth
            />
            <TextField
              label="Type"
              value={formData.type}
              onChange={(e) => setFormData({ ...formData, type: e.target.value })}
              fullWidth
            />
            <TextField
              label="Planting Date"
              type="date"
              value={formData.plantingDate}
              onChange={(e) => setFormData({ ...formData, plantingDate: e.target.value })}
              fullWidth
              InputLabelProps={{ shrink: true }}
            />
            <TextField
              label="Status"
              value={formData.status}
              onChange={(e) => setFormData({ ...formData, status: e.target.value })}
              fullWidth
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSubmit} variant="contained">
            {selectedPlant ? 'Update' : 'Add'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default PlantManagement; 