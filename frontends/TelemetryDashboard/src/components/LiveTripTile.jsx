import React from 'react'
import {Map, Marker, TileLayer, Popup} from 'react-leaflet'
import styled from 'styled-components'
import FormGroup from '@material-ui/core/FormGroup';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import Checkbox from '@material-ui/core/Checkbox';

const MapContainer = styled.div`
    .leaflet-container {
        height:500px;
        width: 100%;
    }
    border: 1px solid #CACACA;
`;

const MapContainerFooter = styled.div`
    border-top: 1px solid #CACACA;
    padding: 5px;
`;

class LiveTripTile extends React.Component 
{
    constructor(props) {
        super(props);
        this.state = {
            trip: {
                latitude: 52.10,
                longitude: 12.10,
                tripDistance: 0,
                tripTime: 0
            },
            zoom: 16,
            showAllDevices: false,
            devices: []
        }
        this.toggleShowAllDevices = this.toggleShowAllDevices.bind(this);
    }

    isVehicleSelected() {
        return this.props.selectedVehicle !== null && this.props.selectedVehicle !== undefined
    }

    validateDeviceContext(deviceId) {
        return this.isVehicleSelected() && deviceId === this.props.selectedVehicle.deviceId
    }

    //Needs overhaul
    componentDidMount()
    {
        const connection = this.props.hubConnection;
        if (connection === null) {
            return;
        }
        connection.on('vehicleLocation', (location) => {
            var loc = JSON.parse(location)
            
            var filteredDevices = []
            if (this.state.showAllDevices) {
                var filteredDevices = this.state.devices.filter((device) => {
                    if (loc.deviceId !== device.deviceId) {
                        return device
                    }
                })
                filteredDevices = [...filteredDevices, loc]
                this.setState({
                    devices: filteredDevices
                })
            } else if (this.isVehicleSelected() && this.validateDeviceContext(loc.deviceId))
            {
                filteredDevices = [loc]
                this.setState({
                    devices: filteredDevices
                })
            }
        });
    }
    
    toggleShowAllDevices() {
        this.setState({
            showAllDevices: !this.state.showAllDevices
        })
    }

    render() {
        const position = [this.state.trip.latitude, this.state.trip.longitude];
        var vehicleAvailable = this.props.selectedVehicle !== undefined && this.props.selectedVehicle !== null

        var markers = this.state.devices.map((device) => {
            return (
                <Marker key={device.deviceId} position={[device.latitude, device.longitude]}>
                    <Popup>{device.deviceId}</Popup>
                </Marker>
            )
        })

        return (
            <MapContainer>
                <Map center={position} zoom={this.state.zoom}>
                    <TileLayer
                        attribution='&amp;copy <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
                        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                    />
                    {markers}
                </Map>
                <MapContainerFooter>
                    <FormGroup row>
                    <FormControlLabel
                        control={
                        <Checkbox
                            value="showAllDevices"
                            color="primary"
                            onChange={this.toggleShowAllDevices}
                            checked={this.state.showAllDevices}
                        />
                        }
                        label="Show all devices"
                    />
                    </FormGroup>
                </MapContainerFooter>
            </MapContainer>
        )
    }
}

export default LiveTripTile;