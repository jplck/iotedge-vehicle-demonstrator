import React from 'react';
import { HubConnectionBuilder } from '@aspnet/signalr';
import {createGlobalStyle} from 'styled-components'
import {Row, Col, Container} from '@bootstrap-styled/v4'
import LiveTripTile from './components/LiveTripTile'
import SpeedAlertTile from './components/SpeedAlertTile'
import ReactNotification, { store } from 'react-notifications-component'

const GlobalStyle = createGlobalStyle`
  html, body {
    background-color: #ffffff;
    height: 100%;
  }
`;

class App extends React.Component
{
  constructor(props) 
  {
      super(props);

      this.state = {
        odometry: {
          MeasuredSpeed: 0,
          SpeedLimit: 80
        }
      }
  }

  componentDidMount() {

    var hubConnectionRef = new HubConnectionBuilder()
      .withUrl(process.env.REACT_APP_HUB_URL)
      .build();

    hubConnectionRef.start().then(
      this.setState({
        hubConnection: hubConnectionRef
      })
    ).then(
      this.setupListeners(hubConnectionRef)
    )
  }

  setupListeners(hubConnection) {
    hubConnection.on('speedAlerts', (speedAlertInfoMsg) => {
      store.addNotification({
        title: "Speed alert!",
        message: "You have exeeded your speed limit.",
        type: "danger",
        insert: "top",
        container: "top-right",
        animationIn: ["animated", "fadeIn"],
        animationOut: ["animated", "fadeOut"],
        dismiss: {
          duration: 3000,
          onScreen: true
        }
      });
    });
  }

  render() {
    return (
      <React.Fragment>
        <GlobalStyle />
        <ReactNotification />
        <Container>
          <Row>
            <Col className="col-10">
              <LiveTripTile hubConnection={this.state.hubConnection}/>
            </Col>
            <Col className="col-2">
              <SpeedAlertTile />
            </Col>
          </Row>
        </Container>
      </React.Fragment>
    )
  }
}

export default App;
