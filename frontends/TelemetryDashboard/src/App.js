import React from 'react';
import { HubConnectionBuilder } from '@aspnet/signalr';
import {createGlobalStyle} from 'styled-components'
import {Row, Col, Container} from 'react-bootstrap'
import LiveTripTile from './components/LiveTripTile'
import SpeedAlertTile from './components/SpeedAlertTile'
import VehicleSelectorTile from './components/VehicleSelectorTile'
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const GlobalStyle = createGlobalStyle`
  html, body {
    background-color: #ffffff;
    height: 100%;
    margin-top: 20px;
  }
`;

class App extends React.Component
{
  constructor(props) 
  {
      super(props);

      this.state = {
        websocket: null,
        selectedVehicle: null
      }

      this.vehicleSelected = this.vehicleSelected.bind(this);
  }

  componentDidMount() {
    var hubConnectionRef = new HubConnectionBuilder()
      .withUrl(process.env.REACT_APP_HUB_URL, { accessTokenFactory: () => this.props.signalRToken })
      .build();

    hubConnectionRef.start().then(
      this.setState({
        websocket: hubConnectionRef
      })
    )
  }

  vehicleSelected(pairing) {
    this.setState({
      selectedVehicle: pairing
    })
  }

  render() {
    if (this.state.websocket !== null) {
      return (
          <React.Fragment>
            <ToastContainer />
            <GlobalStyle />
            <Container>
              <Row>
                <Col className="col-4">
                  <VehicleSelectorTile onSelect={this.vehicleSelected} websocket={this.state.websocket}/>
                </Col>
                <Col className="col-5">
                  <LiveTripTile selectedVehicle={this.state.selectedVehicle} hubConnection={this.state.websocket}/>
                </Col>
                <Col className="col-3">
                  <SpeedAlertTile websocket={this.state.websocket}/>
                </Col>
              </Row>
            </Container>
          </React.Fragment>
        )
      }
      return (
        <div>Loading...</div>
      )
  }
}

export default App;
