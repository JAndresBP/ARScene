from typing import List
from fastapi import FastAPI
from pydantic import BaseModel 

class Item(BaseModel):
    b64Image: str

class Detection(BaseModel):
    x: int
    y: int
    w: int
    h: int
    label:str
    confidence:float

class DetectionResponse(BaseModel):
    detectionList: List[Detection]


app = FastAPI()

@app.get("/")
async def root():
    return {"message": "Hello World"}


@app.post("/")
async def detectObject(data:Item):
    ba=bytearray(data.b64Image,'utf-8')

    detection = Detection(x=100,y=100,w=50,h=50,label="Test",confidence= 100)
    detectionList = [detection]
    res = DetectionResponse(detectionList=detectionList)
    return res