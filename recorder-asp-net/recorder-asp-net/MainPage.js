import React, {useState, useEffect, useRef} from "react";
import { Button,Form } from "react-bootstrap";
export const MainPage= () =>{
    const [transcript, setTranscript]= useState("");
    const [info, setInfo]= useState("");
    const [en, setEn]= useState(false);
    const [ru, setRu]= useState(true);

    const [start, setStart]= useState(false);
    const [stop, setStop]= useState(true);
    
    const [orig, setOrig]= useState("");
    const [trans, setTrans]= useState("");

    const [stream, setStream] = useState({
        access: false,
        recorder: null,
        error: ""
    });
    const chunks = useRef([]);
    const [recording, setRecording] = useState({
        active: false,
        available: false,
        url: ""
    });
    const onClickRecordStart = () => {
            stream.recorder.start()
            setStart(!start)
            setStop(!stop)
    }
    const onClickRecordStop = () => {
        stream.recorder.stop()
        setStart(!start)
        setStop(!stop)
    }
    function getAccess( ) {
        navigator.mediaDevices
            .getUserMedia({ audio: true })
            .then((mic) => {
             
                let mediaRecorder = new MediaRecorder(mic, {
                        mimeType: "audio/webm"
                    });
                
                const track = mediaRecorder.stream.getTracks()[0];
                track.onended = () => console.log("ended");

                mediaRecorder.onstart = function () {
                    setRecording({
                        active: true,
                        available: false,
                        url: ""
                    });
                };

                mediaRecorder.ondataavailable = function (e ) {
                    console.log("data available");
                    chunks.current.push(e.data);
                };

                mediaRecorder.onstop = async function () {
                    console.log("stopped");

                    const url = URL.createObjectURL(chunks.current[0]);

                    PostAudio(new Blob(chunks.current, { type: 'audio/x-mpeg-3' }));

                    chunks.current = [];

                    setRecording({
                        active: false,
                        available: true,
                        url
                    });
                };

                setStream({
                    ...stream,
                    access: true,
                    recorder: mediaRecorder
                });
                setInfo("Device was found");
            })
            .catch((error) => {
                setInfo(error.message);
                console.log(error);
                setStream({ ...stream, error });
            });
    }

 useEffect(()=>{
    getAccess();
 },[en,ru])


    function PostAudio(blob ) {
   
        let reader = new FileReader();
        reader.readAsDataURL(blob);
        reader.onloadend = function () {
            let base64toBlob  = reader.result  ;
            let formData = new FormData();
          
            formData.append('audiofile', base64toBlob);
            formData.append('lang', en? 'en':'ru');
             
            let xhr = new XMLHttpRequest();
            xhr.open('POST', 'https://localhost:7177/recognize/audio', true);
            xhr.onload = function (e) {
                if (this.status === 200) {
                    setTranscript(this.response);

                    let str = JSON.parse(this.response);
                    setOrig(JSON.parse(this.response)["orig"]);
                    setTrans(JSON.parse(this.response)["trans"]);
                } else {
                    alert('Unable to download excel.')
                }
            };
            xhr.send(formData);
        }
    }
  

    return(
        <div>
         
            <p>{info}</p>    
           
            <Button className="record-on" variant="primary" size="lg" onClick={ onClickRecordStart } disabled={start}>
                Start recording
            </Button>
            <Button className="record-off" variant="primary" size="lg" onClick={ onClickRecordStop } disabled={stop}>
                Stop recording
            </Button>
            <Form style={{width:'300px'}}>
                <Form.Check 
                    type="switch"
                    id="ru-switch"
                    label="Говорю по-русски"
                    checked={ru}
                    onChange={()=>{setEn(!en); setRu(!ru);}}
                />
                <Form.Check 
                    type="switch"
                    id="en-switch"
                    label="Говорю по-английски"
                    checked={en}
                    onChange={()=> {setEn(!en); setRu(!ru);}}
                />
            </Form>
            <Form>
                <Form.Group className="mb-3" controlId="exampleForm.ControlTextarea1">
                    <Form.Label>Result</Form.Label>
                    <Form.Control as="textarea" rows={3} value={transcript}/>
                </Form.Group>
            </Form>
            <h3>{orig}</h3>
            <h3>{trans}</h3>
        </div>
    );
}