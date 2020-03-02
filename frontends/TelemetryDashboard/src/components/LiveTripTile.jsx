import React from 'react'
import styled from 'styled-components'
import FormGroup from '@material-ui/core/FormGroup';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import Checkbox from '@material-ui/core/Checkbox';
import * as atlas from 'azure-maps-control'
import * as service from 'azure-maps-rest'

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
    static MAP_ID = "azmap"
    _dataSource = null

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
            zoomStepSize: 1,
            pitchDegrees: 0,
            showAllDevices: false,
            devices: [],
            controls: []
        }
        this.toggleShowAllDevices = this.toggleShowAllDevices.bind(this);
    }

    initializeAzureMap()
    {
        var map = new atlas.Map(LiveTripTile.MAP_ID, {
            view: 'Auto',
            authOptions: {
                authType: 'subscriptionKey',
                subscriptionKey: this.props.subscriptionKey
            }
        })

        map.events.add("ready", () => {
            this._dataSource = new atlas.source.DataSource();
            map.sources.add(this._dataSource);

            var layer = new atlas.layer.SymbolLayer(this._dataSource);

            map.layers.add(layer);

            this.setupListeners(map, this._dataSource);

            this.addMapControls(map)

            map.setTraffic({
                incidents: false,
                flow: 'relative'
            })

        })
    }

    addMapControls(map)
    {
        map.controls.remove(this.state.controls);
        var controls = []

        controls.push(new atlas.control.ZoomControl({
            zoomDelta: parseFloat(this.state.zoomStepSize),
            style: 'light'
        }));

        controls.push(new atlas.control.PitchControl({
            pitchDegreesDelta: parseFloat(5),
            style: 'light'
        }));

        controls.push(new atlas.control.StyleControl({
            style: 'light',
            mapStyles: 'all'
        }));

        controls.push(new atlas.control.CompassControl({
            rotationDegreesDelta: parseFloat(5),
            style: 'light'
        }));

        this.setState({
            controls: controls
        })

        map.controls.add(controls, {
            position: 'bottom-right'
        });
    }

    isVehicleSelected() {
        return this.props.selectedVehicle !== null && this.props.selectedVehicle !== undefined
    }

    validateDeviceContext(deviceId) {
        return this.isVehicleSelected() && deviceId === this.props.selectedVehicle.deviceId
    }

    shouldComponentUpdate(nextProps, nextState) 
    {
        return true
    }

    setupListeners(map, dataSource)
    {
        const connection = this.props.hubConnection;
        if (connection === null) {
            return;
        }
        connection.on('vehicleLocation', (location) => {
            var loc = JSON.parse(location)

            if (this.validateDeviceContext(loc.deviceId) || this.state.showAllDevices) {

                var point = new atlas.Shape(new atlas.data.Point([loc.longitude, loc.latitude]))

                var filteredDevice = this.state.devices.filter((device) => {
                    return loc.deviceId === device.deviceId
                })

                if (filteredDevice.length === 1) {
                    var point = dataSource.getShapeById(filteredDevice[0].pointRef)
                    point.setCoordinates(new atlas.data.Position(loc.longitude, loc.latitude))
                } else {
                    dataSource.add(point)
                    loc = {...loc, pointRef: point.data.id}

                    this.setState({
                        devices: [...this.state.devices.filter((device) => {
                            return loc.deviceId !== device.deviceId
                        }), loc]
                    })
                }
            }
        });
    }

    componentDidMount()
    {
        this.initializeAzureMap()
    }
    
    toggleShowAllDevices() {
        this._dataSource.clear();
        this.setState({
            showAllDevices: !this.state.showAllDevices,
            devices: []
        })
    }

    render() {
        return (
            <MapContainer>
                <div style={{height: '500px', width: '100%'}} id={LiveTripTile.MAP_ID}></div>
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